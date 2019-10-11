// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : MoveNoteCommand.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notes.Commands.MoveNote {
    using MediatR;

    /// <summary>
    /// Essential a group command
    /// </summary>
    public sealed class MoveNoteCommand : IRequest {
        public int NoteId { get; }
        public int? GroupId { get; }

        public MoveNoteCommand(int noteId, int? groupId) {
            this.NoteId = noteId;
            this.GroupId = groupId;
        }
    }
}
