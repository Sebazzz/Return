// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RequestPerformanceBehaviour.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Common.Behaviours {
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public sealed class RequestPerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> {
        private readonly ICurrentParticipantService _currentParticipantService;
        private readonly ILogger<TRequest> _logger;
        private readonly Stopwatch _timer;

        public RequestPerformanceBehaviour(
            ILogger<TRequest> logger,
            ICurrentParticipantService currentParticipantService
        ) {
            this._timer = new Stopwatch();

            this._logger = logger;
            this._currentParticipantService = currentParticipantService;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next
        ) {
            if (next == null) throw new ArgumentNullException(nameof(next));

            this._timer.Start();

            TResponse response = await next().ConfigureAwait(continueOnCapturedContext: false);

            this._timer.Stop();

            if (this._timer.ElapsedMilliseconds > 500) {
                string name = typeof(TRequest).Name;

                this._logger.LogWarning(
                    message:
                    "Return.App Long running request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@Request}",
                    name,
                    this._timer.ElapsedMilliseconds,
                    (await this._currentParticipantService.GetParticipant().ConfigureAwait(false)).Id,
                    request);
            }

            return response;
        }
    }
}
