using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Return.Web {
    using System;
    using Application;
    using Application.Common.Abstractions;
    using Configuration;
    using Infrastructure;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Middleware.Https;
    using Persistence;
    using Services;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "ASP.NET Core conventions")]
    public class Startup {
        public Startup(IConfiguration configuration) {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services) {
            // App
            services.AddInfrastructure();
            services.AddPersistence();
            services.AddApplication();

            services.AddScoped<ICurrentParticipantService, CurrentParticipantService>();

            // ... Config
            services.Configure<DatabaseOptions>(this.Configuration.GetSection("database"));
            services.AddTransient<IDatabaseOptions>(sp => sp.GetRequiredService<IOptions<DatabaseOptions>>().Value);

            // Framework
            services.AddRazorPages();
            services.AddServerSideBlazor();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory) {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (env == null) throw new ArgumentNullException(nameof(env));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            // Log hosting environment
            {
                ILogger logger = loggerFactory.CreateLogger("Startup");
                logger.LogInformation("Using content root: {0}", env.ContentRootPath);
                logger.LogInformation("Using web root: {0}", env.WebRootPath);
            }

            // Set-up application pipeline
            app.UseCurrentParticipantService();

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseExceptionHandler("/Error");
            }

            app.UseHttps(env);
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints => {
                endpoints.MapBlazorHub();

                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
