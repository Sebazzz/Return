// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RejoinRetrospectiveCommandHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Commands.RejoinRetrospective {
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper.QueryableExtensions;
    using Common;
    using Common.Abstractions;
    using Common.Models;
    using Domain.Entities;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public sealed class RejoinRetrospectiveCommandHandler : IRequestHandler<RejoinRetrospectiveCommand> {
        private readonly IReturnDbContext _returnDbContext;
        private readonly ICurrentParticipantService _currentParticipantService;

        public RejoinRetrospectiveCommandHandler(IReturnDbContext returnDbContext, ICurrentParticipantService currentParticipantService) {
            this._returnDbContext = returnDbContext;
            this._currentParticipantService = currentParticipantService;
        }

        public async Task<Unit> Handle(RejoinRetrospectiveCommand request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            Participant? result = await this._returnDbContext.Participants.AsNoTracking().
                Where(x => x.Retrospective.UrlId.StringId == request.RetroId && x.Id == request.ParticipantId).
                FirstOrDefaultAsync(cancellationToken);

            if (result == null) {
                throw new NotFoundException(nameof(Participant), request.ParticipantId);
            }

            this._currentParticipantService.SetParticipant(
                new CurrentParticipantModel(result.Id, result.Name, result.IsFacilitator));

            return Unit.Value;
        }
    }
}
