// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetRetrospectiveStatusCommand.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospective.Queries.GetRetrospectiveStatus {
    using MediatR;

    public sealed class GetRetrospectiveStatusCommand : IRequest<RetrospectiveStatus> {
        public string RetroId { get; }

        public GetRetrospectiveStatusCommand(string retroId) {
            this.RetroId = retroId;
        }
    }
}
