namespace Return.Application.Common.Behaviours {
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using MediatR.Pipeline;
    using Microsoft.Extensions.Logging;

    public sealed class RequestLogger<TRequest> : IRequestPreProcessor<TRequest> {
        private readonly ILogger _logger;
        private readonly ICurrentParticipantService _currentUserService;

        public RequestLogger(ILogger<TRequest> logger, ICurrentParticipantService currentUserService) {
            this._logger = logger;
            this._currentUserService = currentUserService;
        }

        public async Task Process(TRequest request, CancellationToken cancellationToken) {
            string name = typeof(TRequest).Name;

            this._logger.LogInformation("Return.App Request: {Name} {@UserId} {@Request}",
                name, (await this._currentUserService.GetParticipant().ConfigureAwait(false)).Id, request);
        }
    }
}
