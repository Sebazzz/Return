// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : CreateRetrospectiveCommandValidator.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Commands.CreateRetrospective;

using System;
using Common.Settings;
using FluentValidation;
using Microsoft.Extensions.Options;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "This is a validation rule set.")]
public sealed class CreateRetrospectiveCommandValidator : AbstractValidator<CreateRetrospectiveCommand> {
    public CreateRetrospectiveCommandValidator(IOptions<SecuritySettings> securitySettingsAccessor) {
        if (securitySettingsAccessor == null) throw new ArgumentNullException(nameof(securitySettingsAccessor));

        this.RuleFor(x => x.Title).NotEmpty().MaximumLength(256);
        this.RuleFor(x => x.Passphrase).MaximumLength(512);
        this.RuleFor(x => x.FacilitatorPassphrase).NotEmpty().MaximumLength(512);

        this.RuleFor(x => x.LobbyCreationPassphrase)
            .Equal(securitySettingsAccessor.Value.LobbyCreationPassphrase)
            .When(_ => securitySettingsAccessor.Value.LobbyCreationNeedsPassphrase)
            .WithMessage("Invalid pre-shared passphrase entered needed for creating a retrospective");
    }
}
