namespace Return.Application.Retrospectives.Queries.GetJoinRetrospectiveInfo {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Abstractions;
    using Domain.Entities;
    using Domain.Services;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Services;

    public sealed class GetJoinRetrospectiveInfoQueryHandler : IRequestHandler<GetJoinRetrospectiveInfoQuery, JoinRetrospectiveInfo?> {
        private readonly IReturnDbContext _dbContext;
        private readonly ILogger<GetJoinRetrospectiveInfoQueryHandler> _logger;

        public GetJoinRetrospectiveInfoQueryHandler(IReturnDbContext dbContext, ILogger<GetJoinRetrospectiveInfoQueryHandler> logger) {
            this._dbContext = dbContext;
            this._logger = logger;
        }

        public async Task<JoinRetrospectiveInfo?> Handle(GetJoinRetrospectiveInfoQuery request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            Retrospective retrospective = await this._dbContext.Retrospectives.FindByRetroId(request.RetroId, cancellationToken);
            if (retrospective == null) {
                this._logger.LogWarning($"Retrospective with id {request.RetroId} was not found");

                return null;
            }

            this._logger.LogInformation($"Retrospective with id {request.RetroId} was found");
            return new JoinRetrospectiveInfo(
                retrospective.Title,
                retrospective.HashedPassphrase != null,
                retrospective.IsStarted(),
                retrospective.CurrentStage == RetrospectiveStage.Finished);
        }
    }
}
