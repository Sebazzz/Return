// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : AddNoteCommandValidator.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notes.Commands.AddNote {
    using System.Diagnostics.CodeAnalysis;
    using FluentValidation;

    [SuppressMessage(category: "Naming",
        checkId: "CA1710:Identifiers should have correct suffix",
        Justification = "This is a validation rule set.")]
    public sealed class AddNoteCommandValidator : AbstractValidator<AddNoteCommand> {
        public AddNoteCommandValidator() {
            this.RuleFor(c => c.RetroId).NotEmpty();
            this.RuleFor(c => c.LaneId).NotEmpty();
        }
    }
}
