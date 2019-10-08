// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : SubscriberCollection.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// This is essentially a class that does not take a hard reference on its subscribers while taking concurrency into account
    /// </summary>
    internal sealed class SubscriberCollection<TSubscriber> : IDisposable where TSubscriber : class, ISubscriber {
        private readonly ConcurrentDictionary<Guid, WeakReference<TSubscriber>> _subscribers = new ConcurrentDictionary<Guid, WeakReference<TSubscriber>>();
        private readonly ReaderWriterLockSlim _subscriberCollectionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        public void Subscribe(TSubscriber subscriber) {
            if (subscriber == null) throw new ArgumentNullException(nameof(subscriber));
            this._subscribers.TryAdd(subscriber.UniqueId, new WeakReference<TSubscriber>(subscriber));
        }

        public void Unsubscribe(TSubscriber subscriber) {
            if (subscriber == null) throw new ArgumentNullException(nameof(subscriber));
            this._subscribers.TryRemove(subscriber.UniqueId, out _);
        }

        public IEnumerable<TSubscriber> GetItems() {
            // While we iterate through the queue we need to take note of any dead subscribers 
            var deadSubscribers = new List<Guid>();

            // We need to take a read lock on the queue so at least stuff does not get removed while iterating
            this._subscriberCollectionLock.EnterReadLock();
            try {
                foreach (KeyValuePair<Guid, WeakReference<TSubscriber>> subscriberItem in this._subscribers) {
                    if (!subscriberItem.Value.TryGetTarget(out TSubscriber? subscriber)) {
                        deadSubscribers.Add(subscriberItem.Key);
                    }
                    else {
                        yield return subscriber;
                    }
                }
            }
            finally {
                this._subscriberCollectionLock.ExitReadLock();
            }

            // Remove dead subscribers
            if (deadSubscribers.Count > 0) {
                this._subscriberCollectionLock.EnterWriteLock();

                try {
                    foreach (Guid deadSubscriber in deadSubscribers) {
                        this._subscribers.TryRemove(deadSubscriber, out _);
                    }
                }
                finally {
                    this._subscriberCollectionLock.ExitWriteLock();
                }
            }
        }

        public void Dispose() {
            this._subscriberCollectionLock?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
