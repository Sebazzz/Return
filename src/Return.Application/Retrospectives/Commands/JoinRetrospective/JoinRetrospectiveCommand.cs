// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : JoinRetrospectiveCommand.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Commands.JoinRetrospective {
    using MediatR;
    using Queries.GetParticipantsInfo;

#nullable disable

    public sealed class JoinRetrospectiveCommand : IRequest<ParticipantInfo> {
        public string Name { get; set; }
        public string Color { get; set; }

        public string Passphrase { get; set; }

        public bool JoiningAsFacilitator { get; set; }
        public string RetroId { get; set; }

        public override string ToString() => $"[{nameof(JoinRetrospectiveCommand)}] Join retro {this.RetroId} as {this.Name} (facilitator: {this.JoiningAsFacilitator})";
    }
}
