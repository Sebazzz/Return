// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetJoinRetrospectiveInfoCommand.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Queries.GetJoinRetrospectiveInfo {
    using MediatR;

    public sealed class GetJoinRetrospectiveInfoCommand : IRequest<JoinRetrospectiveInfo?> {
#nullable disable
        public string RetroId { get; set; }
    }
}
