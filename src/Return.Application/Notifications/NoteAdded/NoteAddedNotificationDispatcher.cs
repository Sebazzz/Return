// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteAddedNotificationDispatcher.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.NoteAdded {
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public sealed class NoteAddedNotificationDispatcher : NotificationDispatcher<NoteAddedNotification, INoteAddedSubscriber> {
        public NoteAddedNotificationDispatcher(ILogger<NotificationDispatcher<NoteAddedNotification, INoteAddedSubscriber>> logger) : base(logger)
        {
        }

        protected override Task DispatchCore(
            INoteAddedSubscriber subscriber,
            NoteAddedNotification notification
        ) => subscriber.OnNoteAdded(notification: notification);
    }
}
