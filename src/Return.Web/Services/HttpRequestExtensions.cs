// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : HttpRequestExtensions.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Services {
    using System;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Set of extension methods for Microsoft.AspNetCore.Http.HttpRequest
    /// </summary>
    public static class HttpRequestExtensions {
        /// <summary>Gets http request Uri from request object.</summary>
        /// <param name="request">The <see cref="Microsoft.AspNetCore.Http.HttpRequest" /></param>
        /// <returns>A New Uri object representing request Uri.</returns>
        public static Uri GetUri(this HttpRequest request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }

            if (String.IsNullOrWhiteSpace(request.Scheme)) {
                throw new ArgumentException("Http request Scheme is not specified");
            }

            return new Uri(request.Scheme + "://" + (request.Host.HasValue ? (request.Host.Value.IndexOf(",", StringComparison.Ordinal) > 0 ? "MULTIPLE-HOST" : request.Host.Value) : "UNKNOWN-HOST") + (request.Path.HasValue ? request.Path.Value : String.Empty) + (request.QueryString.HasValue ? request.QueryString.Value : String.Empty));
        }
    }
}
