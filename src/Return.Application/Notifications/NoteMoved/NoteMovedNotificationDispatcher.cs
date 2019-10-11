// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteMovedNotificationDispatcher.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.NoteMoved {
    using System.Threading.Tasks;

    public sealed class NoteMovedNotificationDispatcher : NotificationDispatcher<NoteMovedNotification, INoteMovedSubscriber> {
        protected override Task DispatchCore(
            INoteMovedSubscriber subscriber,
            NoteMovedNotification notification
        ) => subscriber.OnNoteMoved(notification);
    }
}
