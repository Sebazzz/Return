// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : JoinRetrospectiveCommandValidator.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospective.Commands.JoinRetrospective
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Abstractions;
    using Domain.Entities;
    using Domain.Services;
    using FluentValidation;
    using FluentValidation.Results;
    using Microsoft.EntityFrameworkCore;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "This is a validation rule set.")]
    public sealed class JoinRetrospectiveCommandValidator : AbstractValidator<JoinRetrospectiveCommand>
    {
        public JoinRetrospectiveCommandValidator()
        {
            this.RuleFor(e => e.Name).NotEmpty().MaximumLength(256);
            this.RuleFor(e => e.Color).NotEmpty().Matches("^#?([A-F0-9]{2}){3}$", RegexOptions.IgnoreCase);

            this.RuleFor(e => e.Passphrase)
                .Configure(p => p.ApplyCondition((vc) => ((JoinRetrospectiveCommand)vc.Instance).JoiningAsManager))
                .NotEmpty();

            // TODO: passphrase validation: https://github.com/ryanelian/FluentValidation.Blazor/issues/2
            /*this.RuleFor(e => e.Passphrase).
                InjectValidator((sp, context) =>
                {
                    var validatorFactory = sp.GetRequiredService<PassphraseValidatorFactory>();
                    JoinRetrospectiveCommand obj = context.InstanceToValidate;
                    return validatorFactory.MakePassphraseValidator(obj.RetroId, obj.JoiningAsManager, obj.Passphrase);
                });*/
        }
    }

    public sealed class PassphraseValidatorFactory
    {
        private readonly IReturnDbContext _returnDbContext;
        private readonly IPassphraseService _passphraseService;

        private static readonly Expression<Func<Retrospective, string?>> GetManagerHash = r=>r.ManagerHashedPassphrase;
        private static readonly Expression<Func<Retrospective, string?>> GetParticipantHash = r=> r.HashedPassphrase;

        public PassphraseValidatorFactory(IReturnDbContext returnDbContext, IPassphraseService passphraseService)
        {
            this._returnDbContext = returnDbContext;
            this._passphraseService = passphraseService;
        }

        public IValidator<string> MakePassphraseValidator(string retroId, bool isManagerRole, string passphrase)
        {
            Expression<Func<Retrospective, string?>> property = isManagerRole ? GetManagerHash : GetParticipantHash;
            Task<string?> RetrospectiveCallback(CancellationToken ct) => this._returnDbContext.Retrospectives.Where(x => x.UrlId.StringId == retroId).Select(property).FirstOrDefaultAsync(ct);

            return new PassphraseValidator(
                this._passphraseService,
                RetrospectiveCallback,
                passphrase
            );
        }

        private sealed class PassphraseValidator : IValidator<string>
        {
            private readonly IPassphraseService _passphraseService;
            private readonly Func<CancellationToken, Task<string?>> _getter;
            private readonly string _passphrase;

            public PassphraseValidator(IPassphraseService passphraseService, Func<CancellationToken, Task<string?>> getter, string passphrase)
            {
                this._passphraseService = passphraseService;
                this._getter = getter;
                this._passphrase = passphrase;

                this.CascadeMode = CascadeMode.StopOnFirstFailure;
            }

            public ValidationResult Validate(object instance) =>
                this.ValidateAsync(instance).ConfigureAwait(false).GetAwaiter().GetResult();

            public Task<ValidationResult> ValidateAsync(
                object instance,
                CancellationToken cancellation = new CancellationToken()
            ) => this.ValidateAsync((string) instance, cancellation);

            public ValidationResult Validate(ValidationContext context) => this.
                ValidateAsync(context, CancellationToken.None).
                ConfigureAwait(false).
                GetAwaiter().
                GetResult();

            public Task<ValidationResult> ValidateAsync(ValidationContext context, CancellationToken cancellation = new CancellationToken()) => this.ValidateAsync(context.InstanceToValidate, cancellation);

            public IValidatorDescriptor CreateDescriptor() => new ValidatorDescriptor<string>(Array.Empty<IValidationRule>());

            public bool CanValidateInstancesOfType(Type type) => typeof(string) == type;

            public ValidationResult Validate(string instance) =>
                this.ValidateAsync(instance).ConfigureAwait(false).GetAwaiter().GetResult();

            public async Task<ValidationResult> ValidateAsync(
                string instance,
                CancellationToken cancellation = new CancellationToken()
            )
            {
                string? hash = await this._getter(cancellation).ConfigureAwait(false);
                if (hash == null)
                {
                    return new ValidationResult();
                }

                bool isValid = this._passphraseService.ValidatePassphrase(this._passphrase, hash);

                return isValid ? new ValidationResult() : new ValidationResult(
                    new[]
                    {
                        new ValidationFailure(null, "Invalid passphrase")
                    }
                );
            }

            public CascadeMode CascadeMode { get; set; }
        }
    }
}
