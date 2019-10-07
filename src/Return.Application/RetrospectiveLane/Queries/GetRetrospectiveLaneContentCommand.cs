// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetRetrospectiveLaneContent.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.RetrospectiveLanes.Queries {
    using MediatR;

    public sealed class GetRetrospectiveLaneContentCommand : IRequest<RetrospectiveLaneContent> {
        public string RetroId { get; }
        public int LaneId { get; }

        public GetRetrospectiveLaneContentCommand(string retroId, int laneId) {
            this.RetroId = retroId;
            this.LaneId = laneId;
        }
    }
}
