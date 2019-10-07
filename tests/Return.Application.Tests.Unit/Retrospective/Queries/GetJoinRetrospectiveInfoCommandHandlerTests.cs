// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetJoinRetrospectiveInfoCommandTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Retrospective.Queries {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Retrospectives.Queries.GetJoinRetrospectiveInfo;
    using Domain.Entities;
    using Microsoft.Extensions.Logging.Abstractions;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class GetJoinRetrospectiveInfoCommandTests : QueryTestBase {
        [Test]
        public async Task GetJoinRetrospectiveInfoCommandHandler_ReturnsNull_OnRetrospectiveNotFound() {
            // Given
            string retroId = "whatever-whatever";
            var handler = new GetJoinRetrospectiveInfoCommandHandler(this.Context, new NullLogger<GetJoinRetrospectiveInfoCommandHandler>());
            var command = new GetJoinRetrospectiveInfoCommand { RetroId = retroId };

            // When
            var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(false);

            // Then
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetJoinRetrospectiveInfoCommandHandler_ReturnsInfo_OnRetrospectiveFound() {
            // Given
            var retrospective = new Retrospective {
                Title = "Hello",
                CreationTimestamp = DateTimeOffset.Now,
                HashedPassphrase = "hello"
            };
            string retroId = retrospective.UrlId.StringId;
            this.Context.Retrospectives.Add(retrospective);
            await this.Context.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

            var handler = new GetJoinRetrospectiveInfoCommandHandler(this.Context, new NullLogger<GetJoinRetrospectiveInfoCommandHandler>());
            var command = new GetJoinRetrospectiveInfoCommand { RetroId = retroId };

            // When
            var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(false);

            // Then
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Title, Is.EqualTo("Hello"));
            Assert.That(result.IsStarted, Is.False);
            Assert.That(result.IsFinished, Is.False);
            Assert.That(result.NeedsParticipantPassphrase, Is.True);
        }
    }
}
