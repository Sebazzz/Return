// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveVoteStatus.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Votes.Queries {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Models;
    using Notifications.VoteChanged;

    public sealed class RetrospectiveVoteStatus {
        private readonly int _maxVotes;

        public RetrospectiveVoteStatus(int maxVotes) {
            this._maxVotes = maxVotes;
        }

        public List<VoteModel> Votes { get; } = new List<VoteModel>();

        public VoteLookup VotesByParticipant { get; } = new VoteLookup(v => v.ParticipantId, (a, b) => b.IsCast.CompareTo(a.IsCast));
        public VoteLookup VotesByNote { get; } = new VoteLookup(v => v.NoteId, (a, b) => a.Id.CompareTo(b.Id));
        public VoteLookup VotesByNoteGroup { get; } = new VoteLookup(v => v.NoteGroupId, (a, b) => a.Id.CompareTo(b.Id));

        internal void Initialize() {
            this.VotesByParticipant.Initialize(this.Votes);
            this.VotesByNote.Initialize(this.Votes);
            this.VotesByNoteGroup.Initialize(this.Votes);
        }

        public bool CanCastVote(int participantId, int laneId) => this.VotesByParticipant.Get(participantId).Count(x => x.IsCast && x.LaneId == laneId) < this._maxVotes;

        public void Apply(VoteChange notification) {
            if (notification == null) throw new ArgumentNullException(nameof(notification));

            // Take care of the mutation, adding and removing fake uncast votes as necessary
            switch (notification.Mutation) {
                case VoteMutationType.Added:
                    this.Votes.Add(notification.Vote);
                    int uncastVoteIdx = this.Votes.FindIndex(x => x.ParticipantId == notification.Vote.ParticipantId && x.IsCast == false);
                    if (uncastVoteIdx != -1) {
                        this.Votes.RemoveAt(uncastVoteIdx);
                    }

                    this.VotesByParticipant.Add(notification.Vote);
                    this.VotesByNote.Add(notification.Vote);
                    this.VotesByNoteGroup.Add(notification.Vote);
                    break;
                case VoteMutationType.Removed:
                    int voteId = notification.Vote.Id;

                    this.Votes.RemoveAll(v => v.Id == voteId);
                    this.Votes.Add(VoteModel.CreateEmptyFrom(notification.Vote));

                    this.VotesByParticipant.Remove(notification.Vote);
                    this.VotesByNote.Remove(notification.Vote);
                    this.VotesByNoteGroup.Remove(notification.Vote);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(notification));
            }
        }
    }
}
