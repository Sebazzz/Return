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
using Microsoft.AspNetCore.Hosting.Server;
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
using Microsoft.Extensions.Hosting;
using Microsoft.Playwright;
using Serilog;

public sealed class ReturnAppFactory : IDisposable {
    private IHost _webHost;

    /// <summary>
    /// Sqlite in-memory databases are killed as soon as the last referencing connection is killed. Therefore we always
    /// want to hold on while the server might be used. In practice, this is per test fixture.
    /// </summary>
    private SqliteConnection _sqliteConnection;

    private IBrowser _browser;

    public IServer Server {
        get {
            this.EnsureHost();

            return this._webHost.Services.GetRequiredService<IServer>();
        }
    }

    public IServiceProvider Services {
        get {
            this.EnsureHost();

            return this._webHost.Services;
        }
    }

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

    public Uri CreateUri(string path) => new Uri(new Uri(this.Server.Features.Get<IServerAddressesFeature>().Addresses.First()), path);

    public IServiceScope CreateTestServiceScope() => this.Services.CreateScope();

    private string ConnectionString { get; } = (new SqliteConnectionStringBuilder {
        BrowsableConnectionString = true,
        Cache = SqliteCacheMode.Shared,
        Mode = SqliteOpenMode.Memory,
        ForeignKeys = true,
        DataSource = "testdb1"
    }).ToString();

    private void EnsureHost()
    {
        if (this._webHost is not null) return;

        IHostBuilder builder = Host.CreateDefaultBuilder();
        this.ConfigureWebHost(builder);

        this._webHost = builder.Build();
        this._webHost.Start();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Not necessary for tests")]
    private void ConfigureWebHost(IHostBuilder builder) {
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
            .ConfigureLogging(lb => {
                lb.SetMinimumLevel(LogLevel.Trace);
                lb.AddProvider(new TestContextLoggerProvider());

                string logFileName = (TestContext.CurrentContext?.Test.ClassName ?? "test-log") + ".log";
                lb.AddFile(Path.Join(Paths.TestArtifactDir, logFileName));
            })
            .UseSerilog()
            .ConfigureWebHostDefaults(wb =>
            {
                wb.UseStaticWebAssets().UseKestrel(k => k.Listen(endPoint)).UseStartup<Startup>();
            })
            .ConfigureServices(services => {
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

    public void Dispose() {
        this._webHost.StopAsync().GetAwaiter().GetResult();
        this._webHost?.Dispose();
        this._sqliteConnection?.Dispose();
        this._sqliteConnection = null;
    }
}
