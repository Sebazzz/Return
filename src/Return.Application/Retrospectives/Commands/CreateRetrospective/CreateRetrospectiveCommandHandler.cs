// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : CreateRetrospectiveCommandHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Commands.CreateRetrospective {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Abstractions;
    using Domain.Entities;
    using Domain.Services;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using QRCoder;
    using Return.Common;
    using Services;

    public sealed class CreateRetrospectiveCommandHandler : IRequestHandler<CreateRetrospectiveCommand, CreateRetrospectiveCommandResponse> {
        private readonly IReturnDbContext _returnDbContext;
        private readonly ISystemClock _systemClock;
        private readonly IUrlGenerator _urlGenerator;
        private readonly IPassphraseService _passphraseService;
        private readonly ILogger<CreateRetrospectiveCommandHandler> _logger;

        public CreateRetrospectiveCommandHandler(IReturnDbContext returnDbContext, IPassphraseService passphraseService, ISystemClock systemClock, IUrlGenerator urlGenerator, ILogger<CreateRetrospectiveCommandHandler> logger) {
            this._returnDbContext = returnDbContext;
            this._passphraseService = passphraseService;
            this._systemClock = systemClock;
            this._urlGenerator = urlGenerator;
            this._logger = logger;
        }

        public async Task<CreateRetrospectiveCommandResponse> Handle(CreateRetrospectiveCommand request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            string? HashOptionalPassphrase(string? plainText) {
                return !String.IsNullOrEmpty(plainText) ? this._passphraseService.CreateHashedPassphrase(plainText) : null;
            }

            using var qrCodeGenerator = new QRCodeGenerator();
            var retrospective = new Retrospective {
                CreationTimestamp = this._systemClock.CurrentTimeOffset,
                Title = request.Title,
                HashedPassphrase = HashOptionalPassphrase(request.Passphrase),
                FacilitatorHashedPassphrase = HashOptionalPassphrase(request.FacilitatorPassphrase) ?? throw new InvalidOperationException("No facilitator passphrase given"),
            };

            this._logger.LogInformation($"Creating new retrospective with id {retrospective.UrlId}");

            string retroLocation = this._urlGenerator.GenerateUrlToRetrospectiveLobby(retrospective.UrlId).ToString();
            var payload = new PayloadGenerator.Url(retroLocation);
            var result = new CreateRetrospectiveCommandResponse(
                retrospective.UrlId,
                new QrCode(qrCodeGenerator.CreateQrCode(payload.ToString(), QRCodeGenerator.ECCLevel.L)),
                retroLocation);

            this._returnDbContext.Retrospectives.Add(retrospective);

            await this._returnDbContext.SaveChangesAsync(cancellationToken);

            return result;
        }
    }
}
