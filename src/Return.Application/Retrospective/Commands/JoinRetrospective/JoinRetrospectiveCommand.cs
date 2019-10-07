// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : JoinRetrospectiveCommand.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospective.Commands.JoinRetrospective {
    using MediatR;

#nullable disable

    public sealed class JoinRetrospectiveCommand : IRequest {
        public string Name { get; set; }
        public string Color { get; set; }

        public string Passphrase { get; set; }

        public bool JoiningAsManager { get; set; }
        public string RetroId { get; set; }
    }
}
