// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ReturnAppFactory.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common;

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using Application.Common.Abstractions;
using Configuration;
using Domain.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Persistence;
using Return.Application.App.Commands.SeedBaseData;
using Return.Common;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;

public sealed class ReturnAppFactory : WebApplicationFactory<Startup> {
    private IWebHost _webHost;

    /// <summary>
    /// Sqlite in-memory databases are killed as soon as the last referencing connection is killed. Therefore we always
    /// want to hold on while the server might be used. In practice, this is per test fixture.
    /// </summary>
    private SqliteConnection _sqliteConnection;

    private IBrowser _browser;

    public ReturnAppFactory() {
        this._sqliteConnection = new SqliteConnection(this.ConnectionString);
        this._sqliteConnection.Open();
    }

    public async Task<IBrowserContext> CreateBrowserContext()
    {
        IBrowser browser = await this.CreateBrowser();
        return await browser.NewContextAsync(new()
        {
            ViewportSize = new()
            {
                Width = 1920,
                Height = 1080
            }
        });
    }

    private async ValueTask<IBrowser> CreateBrowser()
    {
        if (this._browser is not null) return this._browser;

        bool debugMode = String.IsNullOrEmpty(Environment.GetEnvironmentVariable("MOZ_HEADLESS"));

        if (!Directory.Exists(Paths.TracesDirectory)) Directory.CreateDirectory(Paths.TracesDirectory);

        IPlaywright playwright = await Playwright.CreateAsync();
        IBrowser browser = await playwright.Firefox.LaunchAsync(new()
        {
            Headless = !debugMode,
            SlowMo = debugMode ? 10 : null,
            TracesDir = Paths.TracesDirectory
        });

        return this._browser = browser;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "IPageObject is disposable itself")]
    public async Task<TPageObject> CreatePageObject<TPageObject>() where TPageObject : IPageObject, new() {
        TPageObject pageObject = Activator.CreateInstance<TPageObject>();

        IBrowserContext browserContext = await this.CreateBrowserContext();
        await browserContext.NewPageAsync();

        pageObject.SetBrowserContext(browserContext);

        return pageObject;
    }

    public int GetId<TEntity>(Func<DbSet<TEntity>, int> query) where TEntity : class {
        using IServiceScope scope = this.CreateTestServiceScope();

        var returnDbContext = scope.ServiceProvider.GetRequiredService<ReturnDbContext>();
        DbSet<TEntity> dbSet = returnDbContext.Set<TEntity>();

        return query.Invoke(dbSet);
    }

    public int GetLastAddedId<TEntity>() where TEntity : class, IIdPrimaryKey => this.GetId<TEntity>(dbSet => dbSet.OrderByDescending(x => x.Id).Select(x => x.Id).First());

    public Uri CreateUri(string path) => new Uri(this.Server.BaseAddress, path);

    public IServiceScope CreateTestServiceScope() => this.Services.CreateScope();

    private string ConnectionString { get; } = (new SqliteConnectionStringBuilder {
        BrowsableConnectionString = true,
        Cache = SqliteCacheMode.Shared,
        Mode = SqliteOpenMode.Memory,
        ForeignKeys = true,
        DataSource = "testdb1"
    }).ToString();

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Not necessary for tests")]
    protected override void ConfigureWebHost(IWebHostBuilder builder) {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        // Find free TCP port to configure Kestel on
        IPEndPoint endPoint;
        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            socket.Listen(1);
            endPoint = (IPEndPoint)socket.LocalEndPoint;
        }

        // Configure testing to use Kestel and test services
        builder
            .UseStaticWebAssets()
            .ConfigureLogging(lb => {
                lb.SetMinimumLevel(LogLevel.Trace);
                lb.AddProvider(new TestContextLoggerProvider());

                string logFileName = (TestContext.CurrentContext?.Test.ClassName ?? "test-log") + ".log";
                lb.AddFile(Path.Join(Paths.TestArtifactDir, logFileName));
            })
            .ConfigureTestServices(services => {
                // Add a database context using an in-memory database for testing.
                services.RemoveAll<ReturnDbContext>();

                services.AddScoped(sp => {
                    DbContextOptions<ReturnDbContext> options = new DbContextOptionsBuilder<ReturnDbContext>()
                        .UseSqlite(this.ConnectionString)
                        .Options;

                    var context = new ReturnDbContext(options);

                    return context;
                });
                services.ChainInterfaceImplementation<IReturnDbContext, ReturnDbContext>();
                services.ChainInterfaceImplementation<IReturnDbContextFactory, ReturnDbContext>();

                services.Configure<ServerOptions>(s => {
                    s.BaseUrl = "http://localhost:" + endPoint.Port + "/";
                });
            })
            .UseKestrel(k => k.Listen(endPoint))
            .UseEnvironment(environment: "Test");
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "It's fine for testing")]
    public void InitializeBaseData() {
        // Create a scope to obtain a reference to the database
        using IServiceScope scope = this.Services.CreateScope();

        IServiceProvider scopedServices = scope.ServiceProvider;
        var context = scopedServices.GetRequiredService<ReturnDbContext>();
        var logger = scopedServices.
            GetRequiredService<ILogger<ReturnAppFactory>>();

        // Ensure the database is created.
        context.Database.EnsureCreated();

        // ... Base seeding
        try {
            scope.SetNoAuthenticationInfo();

            scope.Send(new SeedBaseDataCommand()).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        catch (Exception ex) {
            logger.LogError(ex, "An error occurred while migrating or initializing the database.");
        }
    }

    protected override TestServer CreateServer(IWebHostBuilder builder) {
        // See: https://github.com/aspnet/AspNetCore/issues/4892
        this._webHost = builder.Build();

        var testServer = new TestServer(new PassthroughWebHostBuilder(this._webHost));
        var address = testServer.Host.ServerFeatures.Get<IServerAddressesFeature>();
        testServer.BaseAddress = new Uri(address.Addresses.First());

        return testServer;
    }

    private sealed class PassthroughWebHostBuilder : IWebHostBuilder {
        private readonly IWebHost _webHost;

        public PassthroughWebHostBuilder(IWebHost webHost) {
            this._webHost = webHost;
        }

        public IWebHost Build() => this._webHost;

        public IWebHostBuilder ConfigureAppConfiguration(Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate) {
            TestContext.WriteLine($"Ignoring call: {typeof(PassthroughWebHostBuilder)}.{nameof(this.ConfigureAppConfiguration)}");
            return this;
        }

        public IWebHostBuilder ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices) {
            TestContext.WriteLine($"Ignoring call: {typeof(PassthroughWebHostBuilder)}.{nameof(ConfigureServices)}");
            return this;
        }

        public IWebHostBuilder ConfigureServices(Action<IServiceCollection> configureServices) {
            TestContext.WriteLine($"Ignoring call: {typeof(PassthroughWebHostBuilder)}.{nameof(ConfigureServices)}");
            return this;
        }

        public string GetSetting(string key) => throw new NotImplementedException();

        public IWebHostBuilder UseSetting(string key, string value) {
            TestContext.WriteLine($"Ignoring call: {typeof(PassthroughWebHostBuilder)}.{nameof(this.UseSetting)}({key}, {value})");
            return this;
        }
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            this._webHost?.Dispose();
            this._sqliteConnection?.Dispose();
            this._sqliteConnection = null;
        }
    }
}
