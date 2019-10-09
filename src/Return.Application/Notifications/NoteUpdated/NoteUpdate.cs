// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteUpdate.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.NoteUpdated {
    using Domain.Entities;

    public class NoteUpdate {
        public int Id { get; }
        public string Text { get; }

        public NoteUpdate(int id, string text) {
            this.Id = id;
            this.Text = text;
        }
        internal static NoteUpdate FromNote(Note note) => new NoteUpdate(note.Id, note.Text);
    }
}
