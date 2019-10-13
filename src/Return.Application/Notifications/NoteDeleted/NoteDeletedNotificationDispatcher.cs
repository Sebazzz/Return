// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : Class1.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.NoteDeleted {
    using System.Threading.Tasks;

    public sealed class
        NoteDeletedNotificationDispatcher : NotificationDispatcher<NoteDeletedNotification, INoteDeletedSubscriber> {
        protected override Task DispatchCore(
            INoteDeletedSubscriber subscriber,
            NoteDeletedNotification notification
        ) => subscriber.OnNoteDeleted(notification: notification);
    }
}
