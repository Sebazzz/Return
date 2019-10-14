// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : CastVoteCommand.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Votes.Commands {
    using MediatR;

    public sealed class CastVoteCommand : IRequest {
        internal VoteEntityType EntityType { get; }

        public int Id { get; }

        internal CastVoteCommand(VoteEntityType entityType, int id) {
            this.EntityType = entityType;
            this.Id = id;
        }

        public static CastVoteCommand ForNote(int id) => new CastVoteCommand(VoteEntityType.Note, id);
        public static CastVoteCommand ForNoteGroup(int id) => new CastVoteCommand(VoteEntityType.NoteGroup, id);
    }

    internal enum VoteEntityType {
        Note,
        NoteGroup
    }
}
