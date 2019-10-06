namespace Return.Web.Middleware {
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Services;

    public sealed class SiteUrlDetectionMiddleware {
        private readonly ISiteUrlDetectionService _siteUrlDetectionService;
        private readonly RequestDelegate _next;

        public SiteUrlDetectionMiddleware(ISiteUrlDetectionService siteUrlDetectionService, RequestDelegate next) {
            this._siteUrlDetectionService = siteUrlDetectionService;
            this._next = next;
        }

        public Task Invoke(HttpContext context) {
            this._siteUrlDetectionService.Update(context);

            return this._next.Invoke(context);
        }
    }
}
