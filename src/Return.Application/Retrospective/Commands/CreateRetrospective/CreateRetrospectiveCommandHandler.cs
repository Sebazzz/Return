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

    public sealed class CreateRetrospectiveCommandHandler : IRequestHandler<CreateRetrospectiveCommand, CreateRetrospectiveCommandResponse> {
        private readonly IReturnDbContext _returnDbContext;
        private readonly IPassphraseService _passphraseService;

        public CreateRetrospectiveCommandHandler(IReturnDbContext returnDbContext, IPassphraseService passphraseService) {
            this._returnDbContext = returnDbContext;
            this._passphraseService = passphraseService;
        }

        public async Task<CreateRetrospectiveCommandResponse> Handle(CreateRetrospectiveCommand request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            using var qrCodeGenerator = new QRCodeGenerator();
            var retrospective = new Retrospective {
                CreationTimestamp = DateTimeOffset.Now,
                Title = request.Title,
                HashedPassphrase = !String.IsNullOrEmpty(request.Passphrase)
                    ? this._passphraseService.CreateHashedPassphrase(request.Passphrase)
                    : null
            };

            this._returnDbContext.Retrospectives.Add(retrospective);

            await this._returnDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new CreateRetrospectiveCommandResponse(
                retrospective.UrlId,
                new QrCode(qrCodeGenerator.CreateQrCode(request.Passphrase, QRCodeGenerator.ECCLevel.L)));
        }
    }
}
