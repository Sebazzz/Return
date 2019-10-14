// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ScopeSafeMediatorDecorator.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Services {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public sealed class ScopeSafeMediatorDecorator : IMediator, IDisposable {
        private readonly IMediator _mediator;
        private readonly SemaphoreSlim _lock;
        private readonly ILogger<ScopeSafeMediatorDecorator> _logger;

        public ScopeSafeMediatorDecorator(IMediator mediator, ILogger<ScopeSafeMediatorDecorator> logger) {
            this._mediator = mediator;
            this._logger = logger;
            this._lock = new SemaphoreSlim(1, 1);
        }

        public async Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = new CancellationToken()
        ) {
            try {
                await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);

                return await this._mediator.Send(request, cancellationToken).ConfigureAwait(false);
            }
            finally {
                try
                {
                    this._lock.Release();
                } catch (ObjectDisposedException ex)
                {
                    this._logger.LogWarning(ex, "Semaphore was already disposed - this may happen after a crash.");
                }
            }
        }

        public Task Publish(object notification, CancellationToken cancellationToken = new CancellationToken()) => this._mediator.Publish(notification, cancellationToken);

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = new CancellationToken()) where TNotification : INotification => this._mediator.Publish(notification, cancellationToken);

        public void Dispose() => this._lock?.Dispose();
    }
}
