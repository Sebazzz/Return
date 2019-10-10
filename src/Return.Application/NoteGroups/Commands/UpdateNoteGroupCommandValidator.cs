// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : UpdateNoteGroupCommandValidator.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.NoteGroups.Commands {
    using System.Diagnostics.CodeAnalysis;
    using FluentValidation;

    [SuppressMessage(category: "Naming",
        checkId: "CA1710:Identifiers should have correct suffix",
        Justification = "This is a validation rule set.")]
    public sealed class UpdateNoteGroupCommandValidator : AbstractValidator<UpdateNoteGroupCommand> {
        public UpdateNoteGroupCommandValidator() {
            this.RuleFor(x => x.RetroId).NotEmpty();
            this.RuleFor(x => x.Id).NotEmpty();
        }
    }
}
