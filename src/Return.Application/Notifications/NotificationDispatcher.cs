// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NotificationDispatcher.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications {
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;

    public abstract class NotificationDispatcher<TNotification, TSubscriber> : INotificationHandler<TNotification>, INotificationSubscription<TSubscriber>, IDisposable where TNotification : INotification where TSubscriber : class, ISubscriber {
        private readonly SubscriberCollection<TSubscriber> _subscriberCollection = new SubscriberCollection<TSubscriber>();

        public Task Dispatch(TNotification notification, CancellationToken cancellationToken) {
            if (notification == null) throw new ArgumentNullException(nameof(notification));

            return Task.WhenAll(this._subscriberCollection.GetItems().Select(subscriber => this.DispatchCore(subscriber, notification))).WithCancellation(cancellationToken);
        }

        protected abstract Task DispatchCore(TSubscriber subscriber, TNotification notification);

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this._subscriberCollection?.Dispose();
            }
        }

        public void Dispose() {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        public void Subscribe(TSubscriber subscriber) {
            if (subscriber == null) throw new ArgumentNullException(nameof(subscriber));
            this._subscriberCollection.Subscribe(subscriber);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1033:Interface methods should be callable by child types", Justification = "Direct dispatch")]
        Task INotificationHandler<TNotification>.Handle(TNotification notification, CancellationToken cancellationToken) {
            if (notification == null) throw new ArgumentNullException(nameof(notification));
            return this.Dispatch(notification, cancellationToken);
        }
    }
}
