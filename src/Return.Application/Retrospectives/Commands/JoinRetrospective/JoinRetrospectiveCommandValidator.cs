// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : JoinRetrospectiveCommandValidator.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Commands.JoinRetrospective {
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;
    using Common.Abstractions;
    using Domain.Entities;
    using Domain.Services;
    using FluentValidation;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "This is a validation rule set.")]
    public sealed class JoinRetrospectiveCommandValidator : AbstractValidator<JoinRetrospectiveCommand> {
        private static readonly Expression<Func<Retrospective, string?>> GetFacilitatorHash = r => r.FacilitatorHashedPassphrase;
        private static readonly Expression<Func<Retrospective, string?>> GetParticipantHash = r => r.HashedPassphrase;

        private readonly IReturnDbContextFactory _returnDbContext;
        private readonly IPassphraseService _passphraseService;

        public JoinRetrospectiveCommandValidator(IReturnDbContextFactory returnDbContext, IPassphraseService passphraseService) {
            this._returnDbContext = returnDbContext;
            this._passphraseService = passphraseService;

            this.RuleFor(e => e.Name).NotEmpty().MaximumLength(256);
            this.RuleFor(e => e.Color).NotEmpty()
                .WithMessage("Please select a color")
                .Matches("^#?([A-F0-9]{2}){3}$", RegexOptions.IgnoreCase)
                .WithMessage("Please select a color");

            this.RuleFor(e => e.Passphrase)
                .NotEmpty()
                .When(x => x.JoiningAsFacilitator);

            // Passphrase validation
            this.RuleFor(e => e.Passphrase).
                Must((obj, passphrase) => this.MustBeAValidPassphrase(obj.RetroId, obj.JoiningAsFacilitator, obj.Passphrase))
                .WithMessage("This passphrase is not valid. Please try again.");
        }

        private bool MustBeAValidPassphrase(string retroId, in bool isFacilitatorRole, string passphrase) {
            using IReturnDbContext dbContext = this._returnDbContext.CreateForEditContext();

            Expression<Func<Retrospective, string?>> property = isFacilitatorRole ? GetFacilitatorHash : GetParticipantHash;
            string? hash = dbContext.Retrospectives.Where(x => x.UrlId.StringId == retroId).Select(property).FirstOrDefault();

            if (hash == null) {
                return true;
            }

            if (String.IsNullOrEmpty(passphrase)) {
                return false;
            }

            return this._passphraseService.ValidatePassphrase(passphrase, hash);
        }
    }
}
