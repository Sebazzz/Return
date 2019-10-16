// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : InitiateVotingStageCommandValidator.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.RetrospectiveWorkflows.Commands {
    using FluentValidation;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming",
        "CA1710:Identifiers should have correct suffix",
        Justification = "This is a validation rule set.")]
    public sealed class InitiateVotingStageCommandValidator : AbstractTimedStageCommandValidator<InitiateVotingStageCommand> {
        public InitiateVotingStageCommandValidator() {
            this.RuleFor(x => x.VotesPerGroup)
                .GreaterThan(0)
                .LessThan(8)
                .WithName("Number of votes per lane");
        }
    }
}
