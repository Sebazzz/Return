// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : CreateRetrospectiveCommandValidator.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Commands.CreateRetrospective {
    using FluentValidation;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "This is a validation rule set.")]
    public sealed class CreateRetrospectiveCommandValidator : AbstractValidator<CreateRetrospectiveCommand> {
        public CreateRetrospectiveCommandValidator() {
            this.RuleFor(x => x.Title).NotEmpty().MaximumLength(256);
            this.RuleFor(x => x.Passphrase).MaximumLength(512);
            this.RuleFor(x => x.FacilitatorPassphrase).NotEmpty().MaximumLength(512);
        }
    }
}
