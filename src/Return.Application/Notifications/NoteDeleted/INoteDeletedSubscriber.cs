// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : INoteDeletedSubscriber.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.NoteDeleted {
    using System.Threading.Tasks;

    public interface INoteDeletedSubscriber : ISubscriber {
        Task OnNoteDeleted(NoteDeletedNotification notification);
    }
}
