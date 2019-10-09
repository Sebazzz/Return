// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : AddNoteCommandHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notes.Commands.AddNote {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Common;
    using Common.Abstractions;
    using Common.Models;
    using Common.Security;
    using Domain.Entities;
    using MediatR;
    using Notifications.NoteAdded;
    using Return.Common;
    using Services;

    public sealed class AddNoteCommandHandler : IRequestHandler<AddNoteCommand, RetrospectiveNote> {
        private readonly IReturnDbContextFactory _returnDbContextFactory;
        private readonly ICurrentParticipantService _currentParticipantService;
        private readonly ISecurityValidator _securityValidator;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ISystemClock _systemClock;

        public AddNoteCommandHandler(IReturnDbContextFactory returnDbContextFactory, ICurrentParticipantService currentParticipantService, ISecurityValidator securityValidator, IMediator mediator, IMapper mapper, ISystemClock systemClock) {
            this._returnDbContextFactory = returnDbContextFactory;
            this._currentParticipantService = currentParticipantService;
            this._securityValidator = securityValidator;
            this._mediator = mediator;
            this._mapper = mapper;
            this._systemClock = systemClock;
        }

        public async Task<RetrospectiveNote> Handle(AddNoteCommand request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Get the required entities
            using IReturnDbContext returnDbContext = this._returnDbContextFactory.CreateForEditContext();

            Retrospective retrospective = await returnDbContext.Retrospectives.FindByRetroId(request.RetroId, cancellationToken);
            if (retrospective == null) {
                throw new NotFoundException(nameof(Retrospective), request.RetroId);
            }

            NoteLane noteLane = await returnDbContext.NoteLanes.FindAsync((KnownNoteLane)request.LaneId);
            if (noteLane == null) {
                throw new NotFoundException(nameof(NoteLane), (KnownNoteLane)request.LaneId);
            }

            // Save the note
            CurrentParticipantModel currentParticipant = await this._currentParticipantService.GetParticipant();
            var note = new Note {
                Retrospective = retrospective,
                CreationTimestamp = this._systemClock.CurrentTimeOffset,
                Lane = noteLane,
                Participant = await returnDbContext.Participants.FindAsync(currentParticipant.Id),
                ParticipantId = currentParticipant.Id,
                Text = String.Empty
            };

            await this._securityValidator.EnsureOperation(retrospective, SecurityOperation.AddOrUpdate, note);
            returnDbContext.Notes.Add(note);
            await returnDbContext.SaveChangesAsync(cancellationToken);

            // Return and broadcast
            var broadcastNote = this._mapper.Map<RetrospectiveNote>(note);
            var returnNote = this._mapper.Map<RetrospectiveNote>(note);
            returnNote.IsOwnedByCurrentUser = true;

            // ... Broadcast
            await this._mediator.Publish(new NoteAddedNotification(request.RetroId, request.LaneId, broadcastNote), cancellationToken);

            return returnNote;
        }
    }
}
