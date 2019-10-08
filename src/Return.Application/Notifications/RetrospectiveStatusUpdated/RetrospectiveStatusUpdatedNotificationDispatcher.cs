// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveStatusUpdatedNotificationDispatcher.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.RetrospectiveStatusUpdated {
    using System.Threading.Tasks;

    public sealed class RetrospectiveStatusUpdatedNotificationDispatcher : NotificationDispatcher<
        RetrospectiveStatusUpdatedNotification, IRetrospectiveStatusUpdatedSubscriber> {
        protected override Task DispatchCore(
            IRetrospectiveStatusUpdatedSubscriber subscriber,
            RetrospectiveStatusUpdatedNotification notification
        ) => subscriber.OnRetrospectiveStatusUpdated(notification.RetrospectiveStatus);
    }
}
