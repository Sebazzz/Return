// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetRetrospectiveStatusQuery.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Queries.GetRetrospectiveStatus {
    using MediatR;

    public sealed class GetRetrospectiveStatusQuery : IRequest<RetrospectiveStatus> {
        public string RetroId { get; }

        public GetRetrospectiveStatusQuery(string retroId) {
            this.RetroId = retroId;
        }
    }
}
