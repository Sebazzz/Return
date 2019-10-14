// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : InitiateDiscussionStageCommandHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.RetrospectiveWorkflows.Commands {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common.Abstractions;
    using Common;
    using Domain.Entities;
    using MediatR;

    public sealed class InitiateFinishStageCommandHandler : AbstractStageCommandHandler<InitiateFinishStageCommand> {
        public InitiateFinishStageCommandHandler(IReturnDbContextFactory returnDbContext, IRetrospectiveStatusUpdateDispatcher retrospectiveStatusUpdateDispatcher) : base(returnDbContext, retrospectiveStatusUpdateDispatcher) {
        }

        protected override async Task<Unit> HandleCore(InitiateFinishStageCommand request, Retrospective retrospective, CancellationToken cancellationToken) {
            if (retrospective == null) throw new ArgumentNullException(nameof(retrospective));

            retrospective.CurrentStage = RetrospectiveStage.Finished;

            await this.DbContext.SaveChangesAsync(cancellationToken);

            await this.DispatchUpdate(retrospective, cancellationToken);

            return Unit.Value;
        }
    }
}
