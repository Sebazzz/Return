// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ScopeSafeMediatorDecoratorTests.cs
//  Project         : Return.Web.Tests.Unit
// ******************************************************************************

namespace Return.Web.Tests.Unit.Services {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging.Abstractions;
    using NUnit.Framework;
    using Web.Services;

    [TestFixture]
    public sealed class ScopeSafeMediatorDecoratorTests {
        [Test]
        [Repeat(10)]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public async Task ScopeSafeMediatorDecoratorTest_OnlyAllowsOneThread() {
            // Given
            using var mediator = new ScopeSafeMediatorDecorator(new MediatorWorker(), new NullLogger<ScopeSafeMediatorDecorator>());

            // When
            Task CreateTasks() {
                return Task.Run(() => mediator.Send(new FakeRequest()));
            }

            await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => CreateTasks()));

            // Then
            Assert.Pass("No exception happened");
        }

        private sealed class MediatorWorker : IMediator {
            private int _ref;

            public async Task<TResponse> Send<TResponse>(
                IRequest<TResponse> request,
                CancellationToken cancellationToken = new CancellationToken()
            ) {
                if (Interlocked.CompareExchange(ref this._ref, 1, 0) != 0) {
                    throw new InvalidOperationException("Threading issues!");
                }

                await Task.Delay(10, cancellationToken).ConfigureAwait(false);

                this._ref = 0;

                return (TResponse)Activator.CreateInstance(typeof(TResponse));
            }

            public Task<object> Send(object request, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

            public Task Publish(object notification, CancellationToken cancellationToken = new CancellationToken()) => throw new System.NotImplementedException();

            public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = new CancellationToken()) where TNotification : INotification => throw new System.NotImplementedException();
        }

        private sealed class FakeRequest : IRequest {
        }
    }
}
