// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : Program.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using Application.App.Commands.SeedBaseData;
    using Application.Common.Abstractions;
    using Configuration;
    using MediatR;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Persistence;
    using Services;

    public static class Program {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public static async Task Main(string[] args) {
            IWebHost host = CreateWebHostBuilder(args: args).Build();

            using (IServiceScope scope = host.Services.CreateScope()) {
                IServiceProvider services = scope.ServiceProvider;

                try {
                    var currentParticipantService = (CurrentParticipantService)services.GetRequiredService<ICurrentParticipantService>();
                    currentParticipantService.SetNoHttpContext();

                    var returnDbContext = services.GetRequiredService<ReturnDbContext>();
                    returnDbContext.Initialize();

                    var mediator = services.GetRequiredService<IMediator>();
                    await mediator.Send(new SeedBaseDataCommand());
                }
                catch (Exception ex) {
                    ILogger logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(Program));
                    logger.LogError(ex, "An error occurred while migrating or initializing the database.");
                }
            }

            await host.RunAsync();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We log and exit instead of crash")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Global for testing")]
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args: args)
                .ConfigureServices(ConfigureServerOptions)
                .ConfigureAppConfiguration(cfg => {
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_FORCE_USERSECRETS") == "True") {
                        cfg.AddUserSecrets(typeof(Program).Assembly);
                    }

                    cfg.AddOperatingSpecificConfigurationFolders();
                })
                .ConfigureLogging((wc, logging) => {
                    IWebHostEnvironment env = wc.HostingEnvironment;
                    IConfiguration config = wc.Configuration;

                    Console.WriteLine($"Current environment: {env.EnvironmentName}");

                    logging.AddConfiguration(config.GetSection("Logging"));
                    logging.AddConsole();

                    if (env.IsDevelopment()) {
                        logging.AddDebug();
                    }
                    else {
                        IConfigurationSection fileSection = config.GetSection("Logging").GetSection("File");
                        string? fileName = fileSection?.GetValue<string>("Path");

                        if (!String.IsNullOrEmpty(fileName)) {
                            try {
                                logging.AddFile(fileSection);
                            }
                            catch (Exception ex) {
                                Console.WriteLine($"Failed to add file log to path [{fileName}]: {ex}");
                            }
                        }
                        else {
                            Console.WriteLine("Skipping file logging...");
                        }
                    }
                })
                .UseStartup<Startup>();

        private static void AddOperatingSpecificConfigurationFolders([NotNull] this IConfigurationBuilder cfg) {
            if (cfg == null) throw new ArgumentNullException(nameof(cfg));

            const string configFileName = "config";
            const string iniFileExt = "ini";
            const string jsonFileExt = "json";

            string MakeFilePath(string extension) {
                return EmitConfigSearchMessage(EnvironmentPath.CreatePath(Path.ChangeExtension(configFileName, extension)));
            }

            string EmitConfigSearchMessage(string path) {
                Console.WriteLine("\tLoading configuration from: {0}", path);
                return path;
            }

            cfg.AddJsonFile(MakeFilePath(jsonFileExt), true);
            cfg.AddIniFile(MakeFilePath(iniFileExt), true);
        }

        private static void ConfigureServerOptions(WebHostBuilderContext wc, IServiceCollection sc) {
            sc.Configure<KestrelServerOptions>(options => {
                options.AddServerHeader = false;
                options.UseSystemd();

                var httpsOptions = wc.Configuration.GetSection("server").GetSection("https").Get<HttpsServerOptions>();

                if (httpsOptions?.CertificatePath != null) {
                    options.Listen(IPAddress.Any, 80);
                    options.Listen(IPAddress.Any, 443, opts => {
                        opts.UseHttps(httpsOptions.CertificatePath, httpsOptions.CertificatePassword);
                    });
                }
            });
        }
    }
}
