// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : AddNoteGroupCommandHandlerTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.NoteGroups.Commands {
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common;
    using Application.Common.Models;
    using Application.Common.Security;
    using Application.NoteGroups.Commands;
    using Application.Notifications.NoteLaneUpdated;
    using Domain.Entities;
    using MediatR;
    using NSubstitute;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class AddNoteGroupCommandHandlerTests : QueryTestBase {
        [Test]
        public void AddNoteGroupCommandHandler_InvalidRetroId_ThrowsNotFoundException() {
            // Given
            var handler = new AddNoteGroupCommandHandler(
                this.Context,
                Substitute.For<ISecurityValidator>(),
                this.Mapper,
                Substitute.For<IMediator>()
            );
            var command = new AddNoteGroupCommand("not found", (int)KnownNoteLane.Start);

            // When
            TestDelegate action = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task AddNoteGroupCommandHandler_InvalidLaneId_ThrowsNotFoundException() {
            // Given
            var handler = new AddNoteGroupCommandHandler(
                this.Context,
                Substitute.For<ISecurityValidator>(),
                this.Mapper,
                Substitute.For<IMediator>()
            );

            var retro = new Retrospective {
                CurrentStage = RetrospectiveStage.Grouping,
                FacilitatorHashedPassphrase = "whatever",
                Title = TestContext.CurrentContext.Test.FullName,
                CreationTimestamp = DateTimeOffset.Now,
                Participants = { new Participant { Name = "John" } }
            };
            string retroId = retro.UrlId.StringId;
            this.Context.Retrospectives.Add(retro);
            await this.Context.SaveChangesAsync();

            var command = new AddNoteGroupCommand(retroId, -1);

            // When
            TestDelegate action = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task AddNoteGroupCommandHandler_ValidCommand_CreatesValidObjectAndBroadcast() {
            // Given
            var securityValidator = Substitute.For<ISecurityValidator>();
            var mediatorMock = Substitute.For<IMediator>();
            var handler = new AddNoteGroupCommandHandler(
                this.Context,
                securityValidator,
                this.Mapper,
                mediatorMock
            );


            var retro = new Retrospective {
                CurrentStage = RetrospectiveStage.Writing,
                FacilitatorHashedPassphrase = "whatever",
                Title = TestContext.CurrentContext.Test.FullName,
                CreationTimestamp = DateTimeOffset.Now,
                Participants = { new Participant { Name = "John" } }
            };
            string retroId = retro.UrlId.StringId;
            this.Context.Retrospectives.Add(retro);
            await this.Context.SaveChangesAsync();

            var command = new AddNoteGroupCommand(retroId, (int)KnownNoteLane.Start);

            // When
            RetrospectiveNoteGroup result = await handler.Handle(command, CancellationToken.None);

            // Then
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Title, Is.EqualTo(String.Empty));

            await securityValidator.Received().EnsureOperation(Arg.Is<Retrospective>(r => r.Id == retro.Id), SecurityOperation.AddOrUpdate, Arg.Any<NoteGroup>());

            var broadcastedUpdate = mediatorMock.ReceivedCalls().FirstOrDefault()?.GetArguments()[0] as NoteLaneUpdatedNotification;

            if (broadcastedUpdate == null) {
                Assert.Fail("No broadcast has gone out");
            }

            Assert.That(broadcastedUpdate.LaneId, Is.EqualTo((int)KnownNoteLane.Start));
            Assert.That(broadcastedUpdate.RetroId, Is.EqualTo(command.RetroId));
            Assert.That(broadcastedUpdate.GroupId, Is.EqualTo(result.Id));
        }
    }
}
