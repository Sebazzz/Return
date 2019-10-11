using System;
using System.Collections.Generic;
using System.Text;

namespace Return.Application.Notifications.NoteMoved {
    using MediatR;

    public sealed class NoteMovedNotification : INotification {
        public string RetroId { get; }
        public int LaneId { get; }
        public int NoteId { get; }
        public int? GroupId { get; }

        public NoteMovedNotification(string retroId, int laneId, int noteId, int? groupId) {
            this.RetroId = retroId;
            this.LaneId = laneId;
            this.NoteId = noteId;
            this.GroupId = groupId;
        }
    }
}
