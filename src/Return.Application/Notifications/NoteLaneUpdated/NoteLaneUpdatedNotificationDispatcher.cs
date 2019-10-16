// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteLaneUpdatedNotificationDispatcher.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.NoteLaneUpdated {
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public sealed class NoteLaneUpdatedNotificationDispatcher : NotificationDispatcher<NoteLaneUpdatedNotification, INoteLaneUpdatedSubscriber> {
        public NoteLaneUpdatedNotificationDispatcher(ILogger<NotificationDispatcher<NoteLaneUpdatedNotification, INoteLaneUpdatedSubscriber>> logger) : base(logger)
        {
        }

        protected override Task DispatchCore(
            INoteLaneUpdatedSubscriber subscriber,
            NoteLaneUpdatedNotification notification
        ) => subscriber.OnNoteLaneUpdated(notification);
    }
}
