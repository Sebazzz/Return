// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveStatusUpdatedNotificationDispatcher.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.RetrospectiveStatusUpdated {
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public sealed class RetrospectiveStatusUpdatedNotificationDispatcher : NotificationDispatcher<
        RetrospectiveStatusUpdatedNotification, IRetrospectiveStatusUpdatedSubscriber> {
        public RetrospectiveStatusUpdatedNotificationDispatcher(ILogger<NotificationDispatcher<RetrospectiveStatusUpdatedNotification, IRetrospectiveStatusUpdatedSubscriber>> logger) : base(logger)
        {
        }

        protected override Task DispatchCore(
            IRetrospectiveStatusUpdatedSubscriber subscriber,
            RetrospectiveStatusUpdatedNotification notification
        ) => subscriber.OnRetrospectiveStatusUpdated(notification.RetrospectiveStatus);
    }
}
