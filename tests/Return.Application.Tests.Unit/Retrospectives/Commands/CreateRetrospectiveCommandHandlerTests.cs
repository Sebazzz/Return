// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : CreateRetrospectiveCommandHandlerTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Retrospectives.Commands {
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Retrospectives.Commands.CreateRetrospective;
    using Domain.Services;
    using Domain.ValueObjects;
    using Microsoft.Extensions.Logging.Abstractions;
    using NSubstitute;
    using NUnit.Framework;
    using Return.Common;
    using Services;
    using Support;

    [TestFixture]
    public sealed class CreateRetrospectiveCommandHandlerTests : CommandTestBase {
        [Test]
        public async Task Handle_GivenValidRequest_ShouldSaveRetrospectiveWithHash() {
            // Given
            var passphraseService = Substitute.For<IPassphraseService>();
            var systemClock = Substitute.For<ISystemClock>();
            var urlGenerator = Substitute.For<IUrlGenerator>();
            var handler = new CreateRetrospectiveCommandHandler(this.Context, passphraseService, systemClock, urlGenerator, new NullLogger<CreateRetrospectiveCommandHandler>());

            passphraseService.CreateHashedPassphrase("anything").Returns("myhash");
            passphraseService.CreateHashedPassphrase("manager password").Returns("managerhash");

            urlGenerator.GenerateUrlToRetrospectiveLobby(Arg.Any<RetroIdentifier>()).Returns(new Uri("https://example.com/retro/1"));

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
            Assert.That(this.Context.Retrospectives.Any(), Is.True);
            Assert.That(this.Context.Retrospectives.First().ManagerHashedPassphrase, Is.EqualTo("managerhash"));
            Assert.That(this.Context.Retrospectives.First().CreationTimestamp, Is.EqualTo(DateTimeOffset.UnixEpoch));
        }
    }
}
