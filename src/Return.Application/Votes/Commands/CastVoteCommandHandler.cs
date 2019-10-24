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
                    return this.HandleNoteVote(request.Id, request.Mutation, cancellationToken);
                case VoteEntityType.NoteGroup:
                    return this.HandleNoteGroupVote(request.Id, request.Mutation, cancellationToken);
                default:
                    throw new ArgumentOutOfRangeException(nameof(request));
            }
        }

        private async Task<Unit> HandleNoteGroupVote(int noteGroupId, VoteMutationType mutationType, CancellationToken cancellationToken) {
            using IReturnDbContext dbContext = this._returnDbContext.CreateForEditContext();

            // Get
            NoteGroup noteGroup = await dbContext.NoteGroups.Include(x => x.Retrospective)
                .Include(x => x.Lane).FirstOrDefaultAsync(x => x.Id == noteGroupId, cancellationToken);

            if (noteGroup == null) {
                throw new NotFoundException(nameof(NoteGroup), noteGroupId);
            }

            NoteVote vote = await (mutationType switch
            {
                VoteMutationType.Added => this.HandleNoteGroupVoteAdd(noteGroup: noteGroup, dbContext: dbContext, cancellationToken: cancellationToken),
                VoteMutationType.Removed => this.HandleNoteGroupVoteRemove(noteGroup: noteGroup, dbContext: dbContext, cancellationToken: cancellationToken),
                _ => throw new InvalidOperationException("Invalid vote mutation type: " + mutationType)
            });

            if (vote == null) {
                return Unit.Value;
            }

            // Broadcast
            await this.Broadcast(vote, mutationType, CancellationToken.None);

            return Unit.Value;
        }

        private async Task<NoteVote?> HandleNoteGroupVoteRemove(NoteGroup noteGroup, IReturnDbContext dbContext, CancellationToken cancellationToken) {
            // Find a vote
            Participant owner = await this.GetParticipant(dbContext);
            NoteVote vote = await dbContext.NoteVotes.FirstOrDefaultAsync(nv =>
                nv.ParticipantId == owner.Id && nv.NoteGroup.Id == noteGroup.Id, cancellationToken);

            if (vote == null) {
                return null;
            }

            await this._securityValidator.EnsureDelete(noteGroup.Retrospective, vote);

            // Save
            dbContext.NoteVotes.Remove(vote);
            await dbContext.SaveChangesAsync(cancellationToken);

            return vote;
        }

        private async Task<NoteVote> HandleNoteGroupVoteAdd(
            NoteGroup noteGroup,
            IReturnDbContext dbContext,
            CancellationToken cancellationToken
        ) {
            // Validate
            var vote = new NoteVote {
                NoteGroup = noteGroup,
                Retrospective = noteGroup.Retrospective,
                Participant = await this.GetParticipant(dbContext)
            };
            await this._securityValidator.EnsureAddOrUpdate(noteGroup.Retrospective, vote);

            // Validate vote action against number of votes
            await ValidateNumberOfVotes(dbContext, noteGroup.Lane.Id, noteGroup.Retrospective, vote.Participant.Id);

            // Save
            dbContext.NoteVotes.Add(vote);
            await dbContext.SaveChangesAsync(cancellationToken);
            return vote;
        }

        private async Task<Unit> HandleNoteVote(int noteId, VoteMutationType mutationType, CancellationToken cancellationToken) {
            using IReturnDbContext dbContext = this._returnDbContext.CreateForEditContext();

            // Get
            Note note = await dbContext.Notes.Include(x => x.Retrospective).Include(x => x.Lane).FirstOrDefaultAsync(x => x.Id == noteId, cancellationToken);

            if (note == null) {
                throw new NotFoundException(nameof(Note), noteId);
            }

            NoteVote vote = await (mutationType switch
            {
                VoteMutationType.Added => this.HandleNoteVoteAdd(note, dbContext: dbContext, cancellationToken: cancellationToken),
                VoteMutationType.Removed => this.HandleNoteVoteRemove(note, dbContext: dbContext, cancellationToken: cancellationToken),
                _ => throw new InvalidOperationException("Invalid vote mutation type: " + mutationType)
            });

            if (vote == null) {
                return Unit.Value;
            }

            // Broadcast
            await this.Broadcast(vote, mutationType, CancellationToken.None);

            return Unit.Value;
        }

        private async Task<NoteVote> HandleNoteVoteAdd(
            Note note,
            IReturnDbContext dbContext,
            CancellationToken cancellationToken
        ) {
            // Validate
            var vote = new NoteVote {
                Note = note,
                Retrospective = note.Retrospective,
                Participant = await this.GetParticipant(dbContext)
            };
            await this._securityValidator.EnsureAddOrUpdate(note.Retrospective, vote);

            // Validate vote action against number of votes
            await ValidateNumberOfVotes(dbContext, note.Lane.Id, note.Retrospective, vote.Participant.Id);

            // Save
            dbContext.NoteVotes.Add(vote);
            await dbContext.SaveChangesAsync(cancellationToken);
            return vote;
        }

        private async Task<NoteVote?> HandleNoteVoteRemove(Note note, IReturnDbContext dbContext, CancellationToken cancellationToken) {
            // Find a vote
            Participant owner = await this.GetParticipant(dbContext);
            NoteVote vote = await dbContext.NoteVotes.FirstOrDefaultAsync(nv => nv.ParticipantId == owner.Id && nv.Note.Id == note.Id, cancellationToken);

            if (vote == null) {
                return null;
            }

            await this._securityValidator.EnsureDelete(note.Retrospective, vote);

            // Save
            dbContext.NoteVotes.Remove(vote);
            await dbContext.SaveChangesAsync(cancellationToken);

            return vote;
        }

        private Task Broadcast(NoteVote vote, VoteMutationType mutation, CancellationToken cancellationToken) {
            var voteModel = this._mapper.Map<VoteModel>(vote);
            var voteChange = new VoteChange(vote.Retrospective.UrlId.StringId, voteModel, mutation);

            return this._mediator.Publish(new VoteChangeNotification(voteChange), cancellationToken);
        }

        private async ValueTask<Participant> GetParticipant(IReturnDbContext dbContext) => await dbContext.Participants.FindAsync((await this._currentParticipantService.GetParticipant()).Id);

        private static async Task ValidateNumberOfVotes(IReturnDbContext dbContext, KnownNoteLane laneId, Retrospective retrospective, int participantId)
        {
            int maxNumberOfVotesPerLane = retrospective.Options.MaximumNumberOfVotes;
            int votesInLane = await dbContext.NoteVotes.CountAsync(v => v.ParticipantId == participantId && (v.Note == null || v.Note.Lane.Id == laneId) && (v.NoteGroup == null || v.NoteGroup.Lane.Id == laneId) && v.Retrospective.Id == retrospective.Id);

            if (votesInLane >= maxNumberOfVotesPerLane)
            {
                throw new InvalidOperationException($"Participant {participantId} already cast {votesInLane} votes (maximum allowed: {maxNumberOfVotesPerLane})");
            }
        }
    }
}
