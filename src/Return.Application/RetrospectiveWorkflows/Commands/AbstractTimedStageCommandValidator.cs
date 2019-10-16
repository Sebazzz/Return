// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : AbstractTimedStageCommandValidator.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.RetrospectiveWorkflows.Commands {
    using FluentValidation;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "This is a validation rule set.")]
    public abstract class AbstractTimedStageCommandValidator<TCommand> : AbstractValidator<TCommand> where TCommand : AbstractTimedStageCommand {
        protected AbstractTimedStageCommandValidator() {
            this.RuleFor(x => x.RetroId).NotEmpty();

            this.RuleFor(x => x.TimeInMinutes)
                .GreaterThan(0)
                .WithName("Time in minutes");
        }
    }
}
