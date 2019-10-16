// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : CreateRetrospectiveCommand.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Commands.CreateRetrospective {
    using MediatR;

    public class CreateRetrospectiveCommand : IRequest<CreateRetrospectiveCommandResponse> {
#nullable disable
        public string Title { get; set; }

        public string FacilitatorPassphrase { get; set; }

#nullable enable
        public string? Passphrase { get; set; }


    }
}
