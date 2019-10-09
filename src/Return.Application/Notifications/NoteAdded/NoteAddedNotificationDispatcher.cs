// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteAddedNotificationDispatcher.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.NoteAdded {
    using System.Threading.Tasks;

    public sealed class
        NoteAddedNotificationDispatcher : NotificationDispatcher<NoteAddedNotification, INoteAddedSubscriber> {
        protected override Task DispatchCore(
            INoteAddedSubscriber subscriber,
            NoteAddedNotification notification
        ) => subscriber.OnNoteAdded(notification: notification);
    }
}
