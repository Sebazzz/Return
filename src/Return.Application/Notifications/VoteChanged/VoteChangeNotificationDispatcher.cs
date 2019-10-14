// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : VoteChangeNotificationDispatcher.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.VoteChanged {
    using System.Threading.Tasks;

    public sealed class VoteChangeNotificationDispatcher : NotificationDispatcher<VoteChangeNotification, IVoteChangeSubscriber> {
        protected override Task DispatchCore(
            IVoteChangeSubscriber subscriber,
            VoteChangeNotification notification
        ) => subscriber.OnVoteChange(notification.VoteChange);
    }
}
