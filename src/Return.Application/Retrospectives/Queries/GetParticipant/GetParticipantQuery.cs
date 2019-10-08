// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetParticipantQuery.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Queries.GetParticipant {
    using GetParticipantsInfo;
    using MediatR;

    public sealed class GetParticipantQuery : IRequest<ParticipantInfo?> {
        public string Name { get; }
        public string RetroId { get; }

        public GetParticipantQuery(string name, string retroId) {
            this.Name = name;
            this.RetroId = retroId;
        }
    }
}
