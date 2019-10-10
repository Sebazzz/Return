// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteLaneUpdatedNotificationDispatcher.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.NoteLaneUpdated {
    using System.Threading.Tasks;

    public sealed class NoteLaneUpdatedNotificationDispatcher : NotificationDispatcher<NoteLaneUpdatedNotification, INoteLaneUpdatedSubscriber> {
        protected override Task DispatchCore(
            INoteLaneUpdatedSubscriber subscriber,
            NoteLaneUpdatedNotification notification
        ) => subscriber.OnNoteLaneUpdated(notification);
    }
}
