// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveStatusUpdateDispatcher.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.RetrospectiveWorkflows.Common {
    using System.Threading;
    using System.Threading.Tasks;
    using Domain.Entities;
    using MediatR;
    using Notifications.RetrospectiveStatusUpdated;
    using Retrospectives.Queries.GetRetrospectiveStatus;

    public interface IRetrospectiveStatusUpdateDispatcher {
        Task DispatchUpdate(Retrospective retrospective, CancellationToken cancellationToken);
    }

    public sealed class RetrospectiveStatusUpdateDispatcher : IRetrospectiveStatusUpdateDispatcher {
        private readonly IRetrospectiveStatusMapper _retrospectiveStatusMapper;
        private readonly IMediator _mediator;

        public RetrospectiveStatusUpdateDispatcher(IRetrospectiveStatusMapper retrospectiveStatusMapper, IMediator mediator) {
            this._retrospectiveStatusMapper = retrospectiveStatusMapper;
            this._mediator = mediator;
        }

        public async Task DispatchUpdate(Retrospective retrospective, CancellationToken cancellationToken) {
            RetrospectiveStatus retrospectiveStatus = await this._retrospectiveStatusMapper.GetRetrospectiveStatus(retrospective, cancellationToken);

            await this._mediator.Publish(new RetrospectiveStatusUpdatedNotification(retrospectiveStatus), cancellationToken);
        }
    }
}
