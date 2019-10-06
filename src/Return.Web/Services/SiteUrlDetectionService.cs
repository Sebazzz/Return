using System;
using System.Collections.Generic;
using System.Linq;

namespace Return.Web.Services {
    using Configuration;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public interface ISiteUrlDetectionService {
        void Update(HttpContext httpContext);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1055:Uri return values should not be strings", Justification = "Merged into uri")]
        string GetSiteUrl();
    }

    public class SiteUrlDetectionService : ISiteUrlDetectionService {
        private readonly ILogger<SiteUrlDetectionService> _logger;
        private string? _siteUrl;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We log it and continue")]
        public SiteUrlDetectionService(IOptions<ServerOptions> serverOptions, ILogger<SiteUrlDetectionService> logger) {
            if (serverOptions == null) throw new ArgumentNullException(nameof(serverOptions));

            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._siteUrl = serverOptions.Value?.BaseUrl;

            if (String.IsNullOrEmpty(this._siteUrl)) {
                this._siteUrl = null;
            }
            else {
                try {
                    this._siteUrl = new Uri(this._siteUrl, UriKind.Absolute).GetLeftPart(UriPartial.Authority);

                    this._logger.LogInformation("Normalized base URL: {0}", this._siteUrl);
                }
                catch (Exception ex) {
                    this._logger.LogError(ex, "Unable to normalize base url");
                }
            }
        }


        public void Update(HttpContext httpContext) {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            if (this._siteUrl == null) {
                this._logger.LogWarning("You have not set an explicit base URL of the application via the [Server:BaseUrl] option. The base URL is now automatically detected. This detection is possibly insecure, and can lead to incorrect results.");

                HttpRequest request = httpContext.Request;
                this._siteUrl = request.GetUri().GetLeftPart(UriPartial.Authority);

                this._logger.LogInformation("Detected base URL: {0}", this._siteUrl);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1055:Uri return values should not be strings", Justification = "Merged into uri")]
        public string GetSiteUrl() => this._siteUrl ?? throw new InvalidOperationException("Base url not determined yet");
    }
}
