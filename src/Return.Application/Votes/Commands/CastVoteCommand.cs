// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : CastVoteCommand.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Votes.Commands {
    using MediatR;
    using Notifications.VoteChanged;

    public sealed class CastVoteCommand : IRequest {
        internal VoteEntityType EntityType { get; }

        public int Id { get; }

        public VoteMutationType Mutation { get; }

        internal CastVoteCommand(VoteEntityType entityType, int id, VoteMutationType mutation) {
            this.EntityType = entityType;
            this.Id = id;
            this.Mutation = mutation;
        }

        public static CastVoteCommand ForNote(int id, VoteMutationType mutation) => new CastVoteCommand(VoteEntityType.Note, id, mutation);
        public static CastVoteCommand ForNoteGroup(int id, VoteMutationType mutation) => new CastVoteCommand(VoteEntityType.NoteGroup, id, mutation);

        public override string ToString() => $"[{nameof(CastVoteCommand)}] {this.Mutation} on {this.EntityType} #{this.Id}";
    }

    internal enum VoteEntityType {
        Note,
        NoteGroup
    }
}
