// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetJoinRetrospectiveInfoQueryHandlerTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Retrospectives.Queries {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Retrospectives.Queries.GetJoinRetrospectiveInfo;
    using Domain.Entities;
    using Microsoft.Extensions.Logging.Abstractions;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class GetJoinRetrospectiveInfoQueryHandlerTests : QueryTestBase {
        [Test]
        public async Task GetJoinRetrospectiveInfoCommandHandler_ReturnsNull_OnRetrospectiveNotFound() {
            // Given
            string retroId = "whatever-whatever";
            var handler = new GetJoinRetrospectiveInfoQueryHandler(this.Context, new NullLogger<GetJoinRetrospectiveInfoQueryHandler>());
            var command = new GetJoinRetrospectiveInfoQuery { RetroId = retroId };

            // When
            var result = await handler.Handle(command, CancellationToken.None);

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
            await this.Context.SaveChangesAsync(CancellationToken.None);

            var handler = new GetJoinRetrospectiveInfoQueryHandler(this.Context, new NullLogger<GetJoinRetrospectiveInfoQueryHandler>());
            var command = new GetJoinRetrospectiveInfoQuery { RetroId = retroId };

            // When
            var result = await handler.Handle(command, CancellationToken.None);

            // Then
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Title, Is.EqualTo("Hello"));
            Assert.That(result.IsStarted, Is.False);
            Assert.That(result.IsFinished, Is.False);
            Assert.That(result.NeedsParticipantPassphrase, Is.True);
        }
    }
}
