// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetShowcaseQueryHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Showcase.Queries {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Models;
    using MediatR;
    using RetrospectiveLanes.Queries;
    using Retrospectives.Queries.GetRetrospectiveStatus;
    using Votes.Queries;

    public sealed class GetShowcaseQueryHandler : IRequestHandler<GetShowcaseQuery, GetShowcaseQueryResult> {
        private readonly IMediator _mediator;

        public GetShowcaseQueryHandler(IMediator mediator) {
            this._mediator = mediator;
        }

        public async Task<GetShowcaseQueryResult> Handle(GetShowcaseQuery request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RetrospectiveStatus retrospective =
                await this._mediator.Send(new GetRetrospectiveStatusQuery(request.RetroId), cancellationToken);

            RetrospectiveVoteStatus voteStatus = (await this._mediator.Send(new GetVotesQuery(request.RetroId), cancellationToken)).VoteStatus;

            var showcaseData = new ShowcaseData();
            foreach (RetrospectiveLane retrospectiveLane in retrospective.Lanes) {
                RetrospectiveLaneContent laneContents =
                    await this._mediator.Send(
                        new GetRetrospectiveLaneContentQuery(request.RetroId, retrospectiveLane.Id), cancellationToken);

                foreach (RetrospectiveNote note in laneContents.Notes) {
                    showcaseData.Items.Add(new ShowcaseItem(note, retrospectiveLane, voteStatus));
                }

                foreach (RetrospectiveNoteGroup noteGroup in laneContents.Groups) {
                    showcaseData.Items.Add(new ShowcaseItem(noteGroup, retrospectiveLane, voteStatus));
                }
            }

            showcaseData.Sort();

            return new GetShowcaseQueryResult(showcaseData);
        }

    }
}
