// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetParticipantsInfoQuery.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Queries.GetParticipantsInfo {
    using MediatR;

    public sealed class GetParticipantsInfoQuery : IRequest<ParticipantsInfoList> {
        public GetParticipantsInfoQuery(string retroId) {
            this.RetroId = retroId;
        }

        public string RetroId { get; }
    }
}
