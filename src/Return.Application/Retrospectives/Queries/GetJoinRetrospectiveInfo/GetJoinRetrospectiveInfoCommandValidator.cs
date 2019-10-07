// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetJoinRetrospectiveInfoCommandValidator.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Queries.GetJoinRetrospectiveInfo {
    using FluentValidation;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "This is a validation rule set.")]
    public sealed class GetJoinRetrospectiveInfoCommandValidator : AbstractValidator<GetJoinRetrospectiveInfoCommand> {
        public GetJoinRetrospectiveInfoCommandValidator() {
            this.RuleFor(x => x.RetroId).NotEmpty();
        }
    }
}
