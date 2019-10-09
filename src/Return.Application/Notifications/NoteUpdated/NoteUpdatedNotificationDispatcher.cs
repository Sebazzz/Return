// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteUpdatedNotificationDispatcher.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.NoteUpdated {
    using System.Threading.Tasks;

    public sealed class NoteUpdatedNotificationDispatcher : NotificationDispatcher<NoteUpdatedNotification, INoteUpdatedSubscriber> {
        protected override Task DispatchCore(
            INoteUpdatedSubscriber subscriber,
            NoteUpdatedNotification notification
        ) => subscriber.OnNoteUpdated(notification.Note);
    }
}
