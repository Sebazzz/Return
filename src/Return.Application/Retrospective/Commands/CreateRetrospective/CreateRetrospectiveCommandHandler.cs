// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : CreateRetrospectiveCommandHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospective.Commands.CreateRetrospective {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Abstractions;
    using Domain.Entities;
    using Domain.Services;
    using MediatR;
    using QRCoder;
    using Return.Common;

    public sealed class CreateRetrospectiveCommandHandler : IRequestHandler<CreateRetrospectiveCommand, CreateRetrospectiveCommandResponse> {
        private readonly IReturnDbContext _returnDbContext;
        private readonly ISystemClock _systemClock;
        private readonly IPassphraseService _passphraseService;

        public CreateRetrospectiveCommandHandler(IReturnDbContext returnDbContext, IPassphraseService passphraseService, ISystemClock systemClock) {
            this._returnDbContext = returnDbContext;
            this._passphraseService = passphraseService;
            this._systemClock = systemClock;
        }

        public async Task<CreateRetrospectiveCommandResponse> Handle(CreateRetrospectiveCommand request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            string? HashOptionalPassphrase(string? plainText)
            {
                return !String.IsNullOrEmpty(plainText)? this._passphraseService.CreateHashedPassphrase(plainText): null;
            }

            using var qrCodeGenerator = new QRCodeGenerator();
            var retrospective = new Retrospective {
                CreationTimestamp = this._systemClock.CurrentTimeOffset,
                Title = request.Title,
                HashedPassphrase = HashOptionalPassphrase(request.Passphrase),
                ManagerHashedPassphrase = HashOptionalPassphrase(request.ManagerPassphrase) ?? throw new InvalidOperationException("No manager passphrase given"),
            };

            this._returnDbContext.Retrospectives.Add(retrospective);

            await this._returnDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new CreateRetrospectiveCommandResponse(
                retrospective.UrlId,
                new QrCode(qrCodeGenerator.CreateQrCode(request.Passphrase, QRCodeGenerator.ECCLevel.L)));
        }
    }
}
