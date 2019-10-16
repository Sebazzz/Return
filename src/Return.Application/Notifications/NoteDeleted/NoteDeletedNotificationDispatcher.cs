// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : Class1.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.NoteDeleted {
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public sealed class
        NoteDeletedNotificationDispatcher : NotificationDispatcher<NoteDeletedNotification, INoteDeletedSubscriber> {
        public NoteDeletedNotificationDispatcher(ILogger<NotificationDispatcher<NoteDeletedNotification, INoteDeletedSubscriber>> logger) : base(logger) {
        }

        protected override Task DispatchCore(
            INoteDeletedSubscriber subscriber,
            NoteDeletedNotification notification
        ) => subscriber.OnNoteDeleted(notification: notification);
    }
}
