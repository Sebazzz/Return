// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : JoinRetrospectiveCommandHandlerTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Retrospectives.Commands {
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Notifications;
    using Application.Retrospectives.Commands.JoinRetrospective;
    using AutoMapper;
    using Common;
    using Common.Abstractions;
    using Domain.Entities;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using NSubstitute;
    using NSubstitute.ReceivedExtensions;
    using NUnit.Framework;
    using Services;
    using Support;

    [TestFixture]
    public sealed class JoinRetrospectiveCommandHandlerTests : CommandTestBase {
        [Test]
        public void JoinRetrospectiveCommand_ThrowsException_WhenNotFound() {
            // Given
            var command = new JoinRetrospectiveCommand {
                RetroId = "not found"
            };
            var handler = new JoinRetrospectiveCommandHandler(this.Context, Substitute.For<ICurrentParticipantService>(), Substitute.For<IMediator>(), Substitute.For<IMapper>());

            // When
            TestDelegate action = () => handler.Handle(command, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task JoinRetrospectiveCommand_SetsParticipantId_WhenJoiningRetrospective() {
            // Given
            var retro = new Retrospective {
                Title = "What",
                Participants =
                {
                    new Participant {Name = "John", Color = Color.BlueViolet},
                    new Participant {Name = "Jane", Color = Color.Aqua},
                },
                HashedPassphrase = "abef"
            };
            var command = new JoinRetrospectiveCommand {
                RetroId = retro.UrlId.StringId,
                Color = "ABCDEF",
                JoiningAsManager = true,
                Name = "The BOSS",
                Passphrase = "Not relevant"
            };
            this.Context.Retrospectives.Add(retro);
            await this.Context.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

            var mediator = Substitute.For<IMediator>();
            var mapper = Substitute.For<IMapper>();

            var currentParticipantService = Substitute.For<ICurrentParticipantService>();
            var handler = new JoinRetrospectiveCommandHandler(
                this.Context,
                currentParticipantService,
                mediator,
                mapper
            );

            // When
            await handler.Handle(command, CancellationToken.None).ConfigureAwait(false);

            // Then
            currentParticipantService.ReceivedWithAnyArgs(Quantity.Exactly(1))
                .SetParticipant(Arg.Any<int>(), Arg.Any<string>(), true);

            Retrospective checkRetro = await this.Context.Retrospectives.AsNoTracking().
                Include(x => x.Participants).
                FindByRetroId(retro.UrlId.StringId, CancellationToken.None).
                ConfigureAwait(false);

            Assert.That(checkRetro.Participants.Select(x => x.Name), Contains.Item("The BOSS"));

            await mediator.Received().
                Publish(Arg.Any<RetrospectiveJoinedNotification>(), Arg.Any<CancellationToken>()).ConfigureAwait(false);
        }
    }
}
