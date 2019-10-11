// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : MoveNoteValidator.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notes.Commands.MoveNote {
    using System.Diagnostics.CodeAnalysis;
    using FluentValidation;

    [SuppressMessage(category: "Naming", checkId: "CA1710:Identifiers should have correct suffix", Justification = "This is a validation rule set.")]
    public sealed class MoveNoteCommandValidator : AbstractValidator<MoveNoteCommand> {
        public MoveNoteCommandValidator() {
            this.RuleFor(c => c.NoteId).NotEmpty();
            this.RuleFor(c => c.GroupId).NotEqual(0);
        }
    }
}
