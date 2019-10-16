// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteUpdatedNotificationDispatcher.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.NoteUpdated {
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public sealed class NoteUpdatedNotificationDispatcher : NotificationDispatcher<NoteUpdatedNotification, INoteUpdatedSubscriber> {
        public NoteUpdatedNotificationDispatcher(ILogger<NotificationDispatcher<NoteUpdatedNotification, INoteUpdatedSubscriber>> logger) : base(logger)
        {
        }

        protected override Task DispatchCore(
            INoteUpdatedSubscriber subscriber,
            NoteUpdatedNotification notification
        ) => subscriber.OnNoteUpdated(notification.Note);
    }
}
