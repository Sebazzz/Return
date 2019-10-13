// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : DeleteNoteCommandValidator.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notes.Commands.DeleteNote {
    using System.Diagnostics.CodeAnalysis;
    using AddNote;
    using FluentValidation;

    [SuppressMessage(category: "Naming",
        checkId: "CA1710:Identifiers should have correct suffix",
        Justification = "This is a validation rule set.")]
    public sealed class DeleteNoteCommandValidator : AbstractValidator<DeleteNoteCommand> {
        public DeleteNoteCommandValidator() {
            this.RuleFor(c => c.RetroId).NotEmpty();
            this.RuleFor(c => c.NoteId).NotEmpty();
        }
    }
}
