// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : INotificationSubscription.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications {
    public interface INotificationSubscription<in TSubscriber> where TSubscriber : ISubscriber {
        void Subscribe(TSubscriber subscriber);

        void Unsubscribe(TSubscriber subscriber);
    }
}
