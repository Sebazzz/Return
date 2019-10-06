// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : CurrentPartipantService.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Services {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Application.Common.Abstractions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    public class CurrentParticipantService : ICurrentParticipantService {
        private const string ParticipantClaimType = ClaimTypes.NameIdentifier;
        private const string ManagerClaimType = ClaimTypes.Role;
        private const string ManagerClaimContent = "Manager";
        private HttpContext? _httpContext;

        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private bool _hasNoHttpContext;

        public CurrentParticipantService(AuthenticationStateProvider authenticationStateProvider) {
            this._authenticationStateProvider = authenticationStateProvider;
        }

        internal void SetHttpContext(HttpContext httpContext) => this._httpContext = httpContext;

        internal void SetNoHttpContext() => this._hasNoHttpContext = true;

        public async Task<int> GetParticipantId() {
            ClaimsPrincipal user = await this.GetUser().ConfigureAwait(false);

            string? rawParticipantId = user.FindFirstValue(ParticipantClaimType);
            if (String.IsNullOrEmpty(rawParticipantId)) {
                return default;
            }

            if (!Int32.TryParse(rawParticipantId, out int participantId)) {
                return default;
            }

            return participantId;
        }

        public async Task<bool> IsManager() {
            ClaimsPrincipal user = await this.GetUser().ConfigureAwait(false);

            string? rawParticipantId = user.FindFirstValue(ManagerClaimType);
            if (String.IsNullOrEmpty(rawParticipantId)) {
                return default;
            }

            return String.Equals(rawParticipantId, ManagerClaimContent, StringComparison.Ordinal);
        }

        private async ValueTask<ClaimsPrincipal> GetUser() {
            if (this._hasNoHttpContext) {
                return new ClaimsPrincipal();
            }

            AuthenticationState authState = await this._authenticationStateProvider.GetAuthenticationStateAsync().ConfigureAwait(false);

            if (authState != null) {
                return authState.User;
            }

            if (this._httpContext == null) {
                throw new InvalidOperationException("HttpContext not set");
            }

            return this._httpContext.User;
        }
    }

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
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
