// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : CreateRetrospectiveCommandTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Retrospective.CreateRetrospective
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Retrospective.Commands.CreateRetrospective;
    using Domain.Services;
    using NSubstitute;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class CreateRetrospectiveCommandTests : CommandTestBase
    {
        [Test]
        public async Task Handle_GivenValidRequest_ShouldSaveRetrospectiveWithHash()
        {
            // Given
            var passphraseService = Substitute.For<IPassphraseService>();
            var handler = new CreateRetrospectiveCommandHandler(this._context, passphraseService);

            passphraseService.CreateHashedPassphrase(Arg.Any<string>()).Returns("myhash");

            var request = new CreateRetrospectiveCommand
            {
                Passphrase = "anything",
                Title = "Hello"
            };

            // When
            CreateRetrospectiveCommandResponse result = await handler.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Then
            Assert.That(result.Identifier.StringId, Is.Not.Null);
            Assert.That(this._context.Retrospectives.Any(), Is.True);
        }
    }
}
