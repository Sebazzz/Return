// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : UpdateNoteGroupCommandHandlerTests.cs
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
    using Application.Notifications.NoteAdded;
    using Application.Notifications.NoteLaneUpdated;
    using Domain.Entities;
    using MediatR;
    using NSubstitute;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class UpdateNoteGroupCommandHandlerTests : QueryTestBase {
        [Test]
        public void UpdateNoteGroupCommandHandler_InvalidRetroId_ThrowsNotFoundException() {
            // Given
            var handler = new UpdateNoteGroupCommandHandler(
                this.Context,
                Substitute.For<ISecurityValidator>(),
                Substitute.For<IMediator>()
            );
            var command = new UpdateNoteGroupCommand("not found", (int)KnownNoteLane.Start, "Foo");

            // When
            TestDelegate action = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task UpdateNoteGroupCommandHandler_InvalidId_ThrowsNotFoundException() {
            // Given
            var handler = new UpdateNoteGroupCommandHandler(
                this.Context,
                Substitute.For<ISecurityValidator>(),
                Substitute.For<IMediator>()
            );

            var retro = new Retrospective {
                CurrentStage = RetrospectiveStage.Grouping,
                FacilitatorHashedPassphrase = "whatever",
                Title = TestContext.CurrentContext.Test.FullName,
                CreationTimestamp = DateTimeOffset.Now,
                Participants = { new Participant { Name = "John" } },
                NoteGroup = { new NoteGroup { Lane = this.Context.NoteLanes.Find(KnownNoteLane.Continue), Title = "???" } }
            };
            string retroId = retro.UrlId.StringId;
            this.Context.Retrospectives.Add(retro);
            await this.Context.SaveChangesAsync();

            var command = new UpdateNoteGroupCommand(retroId, -1, "!!!");

            // When
            TestDelegate action = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task UpdateNoteGroupCommandHandler_ValidCommand_CreatesValidObjectAndBroadcast() {
            // Given
            var securityValidator = Substitute.For<ISecurityValidator>();
            var mediatorMock = Substitute.For<IMediator>();
            var handler = new UpdateNoteGroupCommandHandler(
                this.Context,
                securityValidator,
                mediatorMock
            );


            var retro = new Retrospective {
                CurrentStage = RetrospectiveStage.Writing,
                FacilitatorHashedPassphrase = "whatever",
                Title = TestContext.CurrentContext.Test.FullName,
                CreationTimestamp = DateTimeOffset.Now,
                Participants = { new Participant { Name = "John" } },
                NoteGroup = { new NoteGroup { Lane = this.Context.NoteLanes.Find(KnownNoteLane.Continue), Title = "???" } }
            };
            string retroId = retro.UrlId.StringId;
            this.Context.Retrospectives.Add(retro);
            await this.Context.SaveChangesAsync();
            NoteGroup noteGroup = retro.NoteGroup.First();

            var command = new UpdateNoteGroupCommand(retroId, noteGroup.Id, "!!!");

            // When
            await handler.Handle(command, CancellationToken.None);

            // Then
            await securityValidator.Received().EnsureOperation(Arg.Is<Retrospective>(r => r.Id == retro.Id), SecurityOperation.AddOrUpdate, Arg.Is<NoteGroup>(g => g.Id == noteGroup.Id));

            var broadcastedUpdate = mediatorMock.ReceivedCalls().FirstOrDefault()?.GetArguments()[0] as NoteLaneUpdatedNotification;

            if (broadcastedUpdate == null) {
                Assert.Fail("No broadcast has gone out");
            }

            Assert.That(broadcastedUpdate.LaneId, Is.EqualTo((int)KnownNoteLane.Continue));
            Assert.That(broadcastedUpdate.RetroId, Is.EqualTo(command.RetroId));
            Assert.That(broadcastedUpdate.GroupId, Is.EqualTo(noteGroup.Id));
        }
    }
}
