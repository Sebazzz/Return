// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : JoinRetrospectiveCommandHandlerTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Retrospectives.Commands {
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common;
    using Application.Common.Abstractions;
    using Application.Common.Models;
    using Application.Notifications.RetrospectiveJoined;
    using Application.Retrospectives.Commands.JoinRetrospective;
    using AutoMapper;
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
        private Retrospective _retrospective;

        [OneTimeSetUp]
        public async Task OneTimeSetUp() {
            var retro = new Retrospective {
                Title = "What",
                Participants =
                {
                    new Participant {Name = "John", Color = Color.BlueViolet},
                    new Participant {Name = "Jane", Color = Color.Aqua},
                },
                HashedPassphrase = "abef"
            };

            this.Context.Retrospectives.Add(retro);
            await this.Context.SaveChangesAsync(CancellationToken.None);

            this._retrospective = retro;
        }

        [Test]
        public void JoinRetrospectiveCommand_ThrowsException_WhenNotFound() {
            // Given
            var command = new JoinRetrospectiveCommand {
                RetroId = "not found"
            };
            var handler = new JoinRetrospectiveCommandHandler(this.Context, Substitute.For<ICurrentParticipantService>(), Substitute.For<IMediator>(), Substitute.For<IMapper>());

            // When
            TestDelegate action = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task JoinRetrospectiveCommand_SetsParticipantId_WhenJoiningRetrospective() {
            // Given
            var retro = this._retrospective ?? throw new InvalidOperationException("OneTimeSetup not executed");

            var mediator = Substitute.For<IMediator>();
            var mapper = Substitute.For<IMapper>();

            var currentParticipantService = Substitute.For<ICurrentParticipantService>();
            var handler = new JoinRetrospectiveCommandHandler(
                this.Context,
                currentParticipantService,
                mediator,
                mapper
            );

            var command = new JoinRetrospectiveCommand {
                RetroId = retro.UrlId.StringId,
                Color = "ABCDEF",
                JoiningAsFacilitator = true,
                Name = "The BOSS",
                Passphrase = "Not relevant"
            };

            // When
            await handler.Handle(command, CancellationToken.None);

            // Then
            currentParticipantService.ReceivedWithAnyArgs(Quantity.Exactly(1))
                .SetParticipant(Arg.Any<CurrentParticipantModel>());

            Retrospective checkRetro = await this.Context.Retrospectives.AsNoTracking().
                Include(x => x.Participants).
                FindByRetroId(retro.UrlId.StringId, CancellationToken.None).
                ConfigureAwait(false);

            Assert.That(checkRetro.Participants.Select(x => x.Name), Contains.Item("The BOSS"));

            await mediator.Received().
                Publish(Arg.Any<RetrospectiveJoinedNotification>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task JoinRetrospectiveCommand_DuplicateJoin_DoesNotCreateNewParticipant() {
            // Given
            var retro = this._retrospective ?? throw new InvalidOperationException("OneTimeSetup not executed");

            var mediator = Substitute.For<IMediator>();
            var mapper = Substitute.For<IMapper>();

            var currentParticipantService = Substitute.For<ICurrentParticipantService>();
            var handler = new JoinRetrospectiveCommandHandler(
                this.Context,
                currentParticipantService,
                mediator,
                mapper
            );

            var command = new JoinRetrospectiveCommand {
                RetroId = retro.UrlId.StringId,
                Color = "ABCDEF",
                JoiningAsFacilitator = true,
                Name = "Duplicate joiner",
                Passphrase = "Not relevant"
            };

            // When
            await handler.Handle(command, CancellationToken.None);

            await handler.Handle(command, CancellationToken.None);

            // Then
            var participants = await this.Context.Retrospectives.
                 SelectMany(x => x.Participants).AsNoTracking().ToListAsync();

            Assert.That(participants.Count(x => x.Name == "Duplicate joiner"), Is.EqualTo(1));
        }
    }
}
