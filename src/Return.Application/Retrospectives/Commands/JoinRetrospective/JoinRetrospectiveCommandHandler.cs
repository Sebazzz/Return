// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : JoinRetrospectiveCommandHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Commands.JoinRetrospective {
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Common;
    using Common.Abstractions;
    using Common.Models;
    using Domain.Entities;
    using Domain.ValueObjects;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Notifications.RetrospectiveJoined;
    using Queries.GetParticipantsInfo;
    using Return.Common;
    using Services;

    public sealed class JoinRetrospectiveCommandHandler : IRequestHandler<JoinRetrospectiveCommand, ParticipantInfo> {
        private readonly IReturnDbContext _returnDbContext;
        private readonly ICurrentParticipantService _currentParticipantService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public JoinRetrospectiveCommandHandler(IReturnDbContext returnDbContext, ICurrentParticipantService currentParticipantService, IMediator mediator, IMapper mapper) {
            this._returnDbContext = returnDbContext;
            this._currentParticipantService = currentParticipantService;
            this._mediator = mediator;
            this._mapper = mapper;
        }

        public async Task<ParticipantInfo> Handle(JoinRetrospectiveCommand request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            Retrospective retrospective = await this._returnDbContext.Retrospectives.FindByRetroId(request.RetroId, cancellationToken);

            if (retrospective == null) {
                throw new NotFoundException(nameof(Retrospective), request.RetroId);
            }

            // Create domain object
            Participant participant = await this.GetOrCreateParticipantAsync(request.RetroId, request.Name, cancellationToken);

            participant.IsFacilitator = request.JoiningAsFacilitator;
            participant.Name = request.Name;
            participant.Retrospective = retrospective;
            participant.Color = new ParticipantColor {
                R = Byte.Parse(request.Color[0..2], NumberStyles.AllowHexSpecifier, Culture.Invariant),
                G = Byte.Parse(request.Color[2..4], NumberStyles.AllowHexSpecifier, Culture.Invariant),
                B = Byte.Parse(request.Color[4..6], NumberStyles.AllowHexSpecifier, Culture.Invariant),
            };

            // Save it
            bool isNew = !this._returnDbContext.Participants.Local.Contains(participant);
            if (isNew) this._returnDbContext.Participants.Add(participant);

            await this._returnDbContext.SaveChangesAsync(cancellationToken);

            // Update auth info
            this._currentParticipantService.SetParticipant(new CurrentParticipantModel(participant.Id, participant.Name, request.JoiningAsFacilitator));

            // Broadcast
            var participantInfo = this._mapper.Map<ParticipantInfo>(participant);

            if (isNew) {
                await this._mediator.Publish(new RetrospectiveJoinedNotification(request.RetroId, participantInfo), cancellationToken);
            }

            return participantInfo;
        }

        private async Task<Participant> GetOrCreateParticipantAsync(string retroId, string name, CancellationToken cancellationToken) {
            Participant? existingParticipant = await this._returnDbContext.Participants.FirstOrDefaultAsync(x => x.Name == name && x.Retrospective.UrlId.StringId == retroId, cancellationToken);

            if (existingParticipant == null) {
                return new Participant();
            }

            return existingParticipant;
        }
    }
}
