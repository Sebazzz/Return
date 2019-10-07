// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveJoinedNotificationDispatcher.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.RetrospectiveJoined {
    using System.Threading.Tasks;

    public sealed class RetrospectiveJoinedNotificationDispatcher : NotificationDispatcher<RetrospectiveJoinedNotification, IRetrospectiveJoinedSubscriber> {
        protected override Task DispatchCore(IRetrospectiveJoinedSubscriber subscriber, RetrospectiveJoinedNotification notification) => subscriber.OnParticipantJoinedRetrospective(notification.ParticipantInfo);
    }
}
