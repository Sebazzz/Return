// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : WebUrlGeneratorService.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Services {
    using System;
    using Application.Services;
    using Domain.ValueObjects;

    public class WebUrlGenerator : IUrlGenerator {
        private readonly ISiteUrlDetectionService _siteUrlDetectionService;

        public WebUrlGenerator(ISiteUrlDetectionService siteUrlDetectionService) {
            this._siteUrlDetectionService = siteUrlDetectionService;
        }

        public Uri GenerateUrlToRetrospectiveLobby(RetroIdentifier urlId) {
            if (urlId == null) throw new ArgumentNullException(nameof(urlId));

            var uriBuilder = new UriBuilder(this._siteUrlDetectionService.GetSiteUrl()) {
                Path = $"/retrospective/{urlId.StringId}/join"
            };

            return uriBuilder.Uri;
        }
    }
}
