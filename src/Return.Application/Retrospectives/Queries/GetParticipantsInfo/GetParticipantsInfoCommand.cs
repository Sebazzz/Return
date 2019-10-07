// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetParticipantsInfoCommand.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Queries.GetParticipantsInfo {
    using MediatR;

    public sealed class GetParticipantsInfoCommand : IRequest<ParticipantsInfoList> {
        public GetParticipantsInfoCommand(string retroId) {
            this.RetroId = retroId;
        }

        public string RetroId { get; }
    }
}
