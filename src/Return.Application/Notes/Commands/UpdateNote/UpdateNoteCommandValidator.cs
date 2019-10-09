// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : UpdateNoteCommandValidator.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notes.Commands.UpdateNote {
    using System.Diagnostics.CodeAnalysis;
    using FluentValidation;

    [SuppressMessage(category: "Naming", checkId: "CA1710:Identifiers should have correct suffix", Justification = "This is a validation rule set.")]
    public sealed class UpdateNoteCommandValidator : AbstractValidator<UpdateNoteCommand> {
        public UpdateNoteCommandValidator() {
            this.RuleFor(x => x.Id).NotEmpty();
        }
    }
}
