// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteMovedNotificationDispatcher.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.NoteMoved {
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public sealed class NoteMovedNotificationDispatcher : NotificationDispatcher<NoteMovedNotification, INoteMovedSubscriber> {
        public NoteMovedNotificationDispatcher(ILogger<NotificationDispatcher<NoteMovedNotification, INoteMovedSubscriber>> logger) : base(logger) {
        }

        protected override Task DispatchCore(
            INoteMovedSubscriber subscriber,
            NoteMovedNotification notification
        ) => subscriber.OnNoteMoved(notification);
    }
}
