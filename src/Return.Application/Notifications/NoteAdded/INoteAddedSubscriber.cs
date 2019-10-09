// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : INoteAddedSubscriber.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.NoteAdded {
    using System.Threading.Tasks;

    public interface INoteAddedSubscriber : ISubscriber {
        Task OnNoteAdded(NoteAddedNotification notification);
    }
}
