// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : CreateRetrospectiveCommandTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Retrospective.CreateRetrospective {
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Retrospective.Commands.CreateRetrospective;
    using Domain.Services;
    using NSubstitute;
    using NUnit.Framework;
    using Return.Common;
    using Support;

    [TestFixture]
    public sealed class CreateRetrospectiveCommandTests : CommandTestBase {
        [Test]
        public async Task Handle_GivenValidRequest_ShouldSaveRetrospectiveWithHash() {
            // Given
            var passphraseService = Substitute.For<IPassphraseService>();
            var systemClock = Substitute.For<ISystemClock>();
            var handler = new CreateRetrospectiveCommandHandler(this._context, passphraseService, systemClock);

            passphraseService.CreateHashedPassphrase("anything").Returns("myhash");
            passphraseService.CreateHashedPassphrase("manager password").Returns("managerhash");

            systemClock.CurrentTimeOffset.Returns(DateTimeOffset.UnixEpoch);

            var request = new CreateRetrospectiveCommand {
                Passphrase = "anything",
                ManagerPassphrase = "manager password",
                Title = "Hello"
            };

            // When
            CreateRetrospectiveCommandResponse result = await handler.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Then
            Assert.That(result.Identifier.StringId, Is.Not.Null);
            Assert.That(this._context.Retrospectives.Any(), Is.True);
            Assert.That(this._context.Retrospectives.First().ManagerHashedPassphrase, Is.EqualTo("managerhash"));
            Assert.That(this._context.Retrospectives.First().CreationTimestamp, Is.EqualTo(DateTimeOffset.UnixEpoch));
        }
    }
}
