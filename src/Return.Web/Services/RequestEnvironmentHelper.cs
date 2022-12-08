// ******************************************************************************
//  © 2020 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RequestEnvironmentHelper.cs
//  Project         : PokerTime.Web
// ******************************************************************************

namespace Return.Web.Services;

using Application.Common.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Middleware;

internal static class RequestEnvironmentHelper {
    public static void UseRequestEnvironmentDetection(this IApplicationBuilder app) {
        IOptions<SecuritySettings> securitySettingsAccessor = app.ApplicationServices.GetRequiredService<IOptions<SecuritySettings>>();

        if (securitySettingsAccessor.Value.EnableProxyMode) {
            app.UseForwardedHeaders(new ForwardedHeadersOptions {
                ForwardedHeaders = ForwardedHeaders.All
            });
        }

        app.UseMiddleware<SiteUrlDetectionMiddleware>();
    }
}
