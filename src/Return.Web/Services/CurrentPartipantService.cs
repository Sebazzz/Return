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
    using Application.Common.Models;
    using Common;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    public class CurrentParticipantService : ICurrentParticipantService {
        private const string ParticipantClaimType = ClaimTypes.NameIdentifier;
        private const string ParticipantNameClaimType = ClaimTypes.Name;
        private const string FacilitatorClaimType = ClaimTypes.Role;
        private const string FacilitatorClaimContent = "Facilitator";
        private HttpContext? _httpContext;

        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private bool _hasNoHttpContext;
        private ClaimsPrincipal? _currentClaimsPrincipal;

        public CurrentParticipantService(AuthenticationStateProvider authenticationStateProvider) {
            this._authenticationStateProvider = authenticationStateProvider ?? throw new ArgumentNullException(nameof(authenticationStateProvider));

            this._authenticationStateProvider.AuthenticationStateChanged += this.OnAuthenticationStateChanged;
        }

        private void OnAuthenticationStateChanged(Task<AuthenticationState> task) =>
            task.ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        this._currentClaimsPrincipal = t.Result?.User;
                    }
                }, TaskScheduler.Current);

        internal void SetHttpContext(HttpContext httpContext)
        {
            this._httpContext = httpContext;
            this._hasNoHttpContext = false;
        }

        internal void SetNoHttpContext() => this._hasNoHttpContext = true;

        public void SetParticipant(CurrentParticipantModel currentParticipant) {
            var hostEnvProvider = this._authenticationStateProvider as IHostEnvironmentAuthenticationStateProvider;

            if (hostEnvProvider == null) {
                return;
            }

            ( int participantId, string? name, bool isFacilitator ) = currentParticipant;

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ParticipantClaimType, participantId.ToString(Culture.Invariant), participantId.GetType().FullName));
            identity.AddClaim(new Claim(ParticipantNameClaimType, name, typeof(string).FullName));
            if (isFacilitator) {
                identity.AddClaim(new Claim(FacilitatorClaimType, FacilitatorClaimContent, FacilitatorClaimContent.GetType().FullName));
            }

            this._currentClaimsPrincipal = new ClaimsPrincipal(identity);

            hostEnvProvider.SetAuthenticationState(Task.FromResult(new AuthenticationState(this._currentClaimsPrincipal)));
        }

        public async ValueTask<CurrentParticipantModel> GetParticipant()
        {
            ClaimsPrincipal user = await this.GetUser().ConfigureAwait(false);

            return new CurrentParticipantModel(
                GetParticipantId(user),
                GetNameLocal(user),
                IsFacilitator(user)
            );
        }

        private async ValueTask<ClaimsPrincipal> GetUser() {
            if (this._hasNoHttpContext) {
                return new ClaimsPrincipal();
            }

            if (this._currentClaimsPrincipal != null) {
                return this._currentClaimsPrincipal;
            }

            AuthenticationState authState = await this._authenticationStateProvider.GetAuthenticationStateAsync().ConfigureAwait(false);

            if (authState != null) {
                this._currentClaimsPrincipal = authState.User;
                return authState.User;
            }

            if (this._httpContext == null) {
                throw new InvalidOperationException("HttpContext not set");
            }

            return this._httpContext.User;
        }

        private static string GetNameLocal(ClaimsPrincipal user) => user.FindFirstValue(ParticipantNameClaimType);

        private static bool IsFacilitator(ClaimsPrincipal user)
        {
            string? rawParticipantId = user.FindFirstValue(FacilitatorClaimType);
            if (String.IsNullOrEmpty(rawParticipantId))
            {
                return default;
            }

            return String.Equals(rawParticipantId, FacilitatorClaimContent, StringComparison.Ordinal);
        }

        private static int GetParticipantId(ClaimsPrincipal user)
        {
            string? rawParticipantId = user.FindFirstValue(ParticipantClaimType);
            if (String.IsNullOrEmpty(rawParticipantId))
            {
                return default;
            }

            if (!Int32.TryParse(rawParticipantId, out int participantId))
            {
                return default;
            }

            return participantId;
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
