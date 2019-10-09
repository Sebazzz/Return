// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : INoteUpdatedSubscriber.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.NoteUpdated {
    using System.Threading.Tasks;

    public interface INoteUpdatedSubscriber : ISubscriber {
        Task OnNoteUpdated(NoteUpdate note);
    }
}
