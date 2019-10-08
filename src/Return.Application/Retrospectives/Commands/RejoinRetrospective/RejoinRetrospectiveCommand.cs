// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RejoinRetrospectiveCommand.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Commands.RejoinRetrospective {
    using MediatR;

    public sealed class RejoinRetrospectiveCommand : IRequest {
        public string RetroId { get; }

        public int ParticipantId { get; }

        public RejoinRetrospectiveCommand(string retroId, int participantId) {
            this.RetroId = retroId;
            this.ParticipantId = participantId;
        }
    }
}
