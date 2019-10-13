// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteDeletedNotification.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.NoteDeleted {
    using MediatR;

    public class NoteDeletedNotification : INotification {
        public NoteDeletedNotification(string retroId, int laneId, in int noteId) {
            this.RetroId = retroId;
            this.LaneId = laneId;
            this.NoteId = noteId;
        }

        public string RetroId { get; }
        public int LaneId { get; }
        public int NoteId { get; }
    }
}
