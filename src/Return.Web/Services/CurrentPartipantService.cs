// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : CurrentPartipantService.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Services {
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Application.Common.Abstractions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    public class CurrentParticipantService : ICurrentParticipantService {
        private const string ParticipantClaimType = ClaimTypes.NameIdentifier;
        private HttpContext? _httpContext;

        internal void SetHttpContext(HttpContext httpContext) {
            this._httpContext = httpContext;
        }

        public int GetParticipantId() {
            if (this._httpContext == null) {
                throw new InvalidOperationException("HttpContext not set");
            }

            string? rawParticipantId = this._httpContext.User.FindFirstValue(ParticipantClaimType);
            if (String.IsNullOrEmpty(rawParticipantId)) {
                return default;
            }

            if (!Int32.TryParse(rawParticipantId, out int participantId)) {
                return default;
            }

            return participantId;
        }
    }

    public class CurrentParticipantServiceHttpContextSetterMiddleware {
        private readonly RequestDelegate _next;

        public CurrentParticipantServiceHttpContextSetterMiddleware(RequestDelegate next) {
            this._next = next;
        }

        public Task InvokeAsync(HttpContext httpContext) {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            var currentParticipantService =
                (CurrentParticipantService)httpContext.RequestServices.
                    GetRequiredService<ICurrentParticipantService>();
            currentParticipantService.SetHttpContext(httpContext);

            return this._next.Invoke(httpContext);
        }
    }

    public static class AppBuilderExtensions {
        public static void UseCurrentParticipantService(this IApplicationBuilder appBuilder) {
            appBuilder.UseMiddleware<CurrentParticipantServiceHttpContextSetterMiddleware>();
        }
    }
}
