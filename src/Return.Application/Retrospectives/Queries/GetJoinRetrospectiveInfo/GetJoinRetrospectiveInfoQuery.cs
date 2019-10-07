// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetJoinRetrospectiveInfoQuery.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Queries.GetJoinRetrospectiveInfo {
    using MediatR;

    public sealed class GetJoinRetrospectiveInfoQuery : IRequest<JoinRetrospectiveInfo?> {
#nullable disable
        public string RetroId { get; set; }
    }
}
