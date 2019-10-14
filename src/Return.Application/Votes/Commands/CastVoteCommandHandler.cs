// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : CastVoteCommandHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Votes.Commands {
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
    using Microsoft.EntityFrameworkCore;
    using Notifications.VoteChanged;

    public sealed class CastVoteCommandHandler : IRequestHandler<CastVoteCommand> {
        private readonly IReturnDbContextFactory _returnDbContext;
        private readonly ICurrentParticipantService _currentParticipantService;
        private readonly ISecurityValidator _securityValidator;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CastVoteCommandHandler(IReturnDbContextFactory returnDbContext, ICurrentParticipantService currentParticipantService, ISecurityValidator securityValidator, IMediator mediator, IMapper mapper) {
            this._returnDbContext = returnDbContext;
            this._currentParticipantService = currentParticipantService;
            this._securityValidator = securityValidator;
            this._mediator = mediator;
            this._mapper = mapper;
        }

        public Task<Unit> Handle(CastVoteCommand request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            switch (request.EntityType) {
                case VoteEntityType.Note:
                    return this.HandleNoteVote(request.Id, cancellationToken);
                case VoteEntityType.NoteGroup:
                    return this.HandleNoteGroupVote(request.Id, cancellationToken);
                default:
                    throw new ArgumentOutOfRangeException(nameof(request));
            }
        }

        private async Task<Unit> HandleNoteGroupVote(int noteGroupId, CancellationToken cancellationToken) {
            using IReturnDbContext dbContext = this._returnDbContext.CreateForEditContext();

            // Get
            NoteGroup noteGroup = await dbContext.NoteGroups.Include(x => x.Retrospective)
                .Include(x => x.Lane).FirstOrDefaultAsync(x => x.Id == noteGroupId, cancellationToken);

            if (noteGroup == null) {
                throw new NotFoundException(nameof(NoteGroup), noteGroupId);
            }

            // Validate
            var vote = new NoteVote {
                NoteGroup = noteGroup,
                Retrospective = noteGroup.Retrospective,
                Participant = await this.GetParticipant(dbContext)
            };
            await this._securityValidator.EnsureAddOrUpdate(noteGroup.Retrospective, vote);

            // Save
            dbContext.NoteVotes.Add(vote);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Broadcast
            await this.Broadcast(vote, VoteMutationType.Added, CancellationToken.None);

            return Unit.Value;
        }

        private async Task<Unit> HandleNoteVote(int noteId, CancellationToken cancellationToken) {
            using IReturnDbContext dbContext = this._returnDbContext.CreateForEditContext();

            // Get
            Note note = await dbContext.Notes.Include(x => x.Retrospective).Include(x => x.Lane).FirstOrDefaultAsync(x => x.Id == noteId, cancellationToken);

            if (note == null) {
                throw new NotFoundException(nameof(Note), noteId);
            }

            // Validate
            var vote = new NoteVote {
                Note = note,
                Retrospective = note.Retrospective,
                Participant = await this.GetParticipant(dbContext)
            };
            await this._securityValidator.EnsureAddOrUpdate(note.Retrospective, vote);

            // Save
            dbContext.NoteVotes.Add(vote);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Broadcast
            await this.Broadcast(vote, VoteMutationType.Added, CancellationToken.None);

            return Unit.Value;
        }

        private Task Broadcast(NoteVote vote, VoteMutationType mutation, CancellationToken cancellationToken) {
            var voteModel = this._mapper.Map<VoteModel>(vote);
            var voteChange = new VoteChange(vote.Retrospective.UrlId.StringId, voteModel, mutation);

            return this._mediator.Publish(new VoteChangeNotification(voteChange), cancellationToken);
        }

        private async ValueTask<Participant> GetParticipant(IReturnDbContext dbContext) => await dbContext.Participants.FindAsync((await this._currentParticipantService.GetParticipant()).Id);
    }
}
