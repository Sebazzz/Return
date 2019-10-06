// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ApplicationBuilderExtensions.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Middleware.Https {
    using System;
    using Configuration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;

    public static class ApplicationBuilderExtensions {
        public static IApplicationBuilder UseHttps(this IApplicationBuilder app, IHostEnvironment hostEnvironment) {
            if (app == null) throw new ArgumentNullException(nameof(app));

            var httpsOptions = app.ApplicationServices.GetService<IOptions<HttpsServerOptions>>();

            if (httpsOptions != null && httpsOptions.Value != null) {
                if (httpsOptions.Value.EnableRedirect) {
                    app.UseHttpsRedirection();
                }

                if (!hostEnvironment.IsDevelopment()) {
                    if (httpsOptions.Value.UseStrongHttps) {
                        app.UseHsts();

                        app.Use((ctx, next) => {
                            ctx.Response.Headers.Append("Content-Security-Policy", "upgrade-insecure-requests");
                            return next.Invoke();
                        });
                    }
                }
            }

            return app;
        }
    }
}
