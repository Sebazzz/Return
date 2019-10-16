// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NotificationDispatcher.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications {
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public abstract class NotificationDispatcher<TNotification, TSubscriber> : INotificationHandler<TNotification>, INotificationSubscription<TSubscriber>, IDisposable where TNotification : INotification where TSubscriber : class, ISubscriber {
        private readonly SubscriberCollection<TSubscriber> _subscriberCollection = new SubscriberCollection<TSubscriber>();
        private readonly ILogger<NotificationDispatcher<TNotification, TSubscriber>> _logger;

        protected NotificationDispatcher(ILogger<NotificationDispatcher<TNotification, TSubscriber>> logger)
        {
            this._logger = logger;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We log and threat this as non-fatal")]
        public async void Dispatch(TNotification notification, CancellationToken cancellationToken) {
            if (notification == null) throw new ArgumentNullException(nameof(notification));

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                this._logger.LogInformation($"Dispatching notification {typeof(TNotification)}");
                await Task.WhenAll(this._subscriberCollection.GetItems().
                        Select(subscriber => this.DispatchCore(subscriber, notification))).
                    WithCancellation(cancellationToken);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Unable to dispatch notification {typeof(TNotification)} due to exception");
            }
            finally
            {
                stopwatch.Stop();
                this._logger.LogInformation(
                    $"Dispatched notification {typeof(TNotification)} in {stopwatch.Elapsed}"
                );
            }
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

        public void Unsubscribe(TSubscriber subscriber) {
            if (subscriber == null) throw new ArgumentNullException(nameof(subscriber));
            this._subscriberCollection.Unsubscribe(subscriber);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1033:Interface methods should be callable by child types", Justification = "Direct dispatch")]
        Task INotificationHandler<TNotification>.Handle(TNotification notification, CancellationToken cancellationToken) {
            if (notification == null) throw new ArgumentNullException(nameof(notification));
            this.Dispatch(notification, cancellationToken);
            return Task.CompletedTask;
        }
    }
}
