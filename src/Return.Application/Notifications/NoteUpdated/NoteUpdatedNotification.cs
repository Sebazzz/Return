// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteUpdatedNotification.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.NoteUpdated {
    using MediatR;

    public sealed class NoteUpdatedNotification : INotification {
        public NoteUpdatedNotification(NoteUpdate note) {
            this.Note = note;
        }

        public NoteUpdate Note { get; }
    }
}
