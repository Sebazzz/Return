// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : DeleteNoteGroupCommandHandlerTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.NoteGroups.Commands {
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common;
    using Application.Common.Security;
    using Application.NoteGroups.Commands;
    using Application.Notifications.NoteLaneUpdated;
    using Domain.Entities;
    using MediatR;
    using NSubstitute;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class DeleteNoteGroupCommandHandlerTests : QueryTestBase {
        [Test]
        public async Task DeleteNoteGroupCommandHandler_InvalidRetroId_SilentlyContinueNoBroadcast() {
            // Given
            var mediator = Substitute.For<IMediator>();
            var handler = new DeleteNoteGroupCommandHandler(
                this.Context,
                Substitute.For<ISecurityValidator>(),
                mediator
            );
            var command = new DeleteNoteGroupCommand("not found", (int)KnownNoteLane.Start);

            // When
            await handler.Handle(command, CancellationToken.None);

            // Then
            Assert.That(mediator.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public async Task DeleteNoteGroupCommandHandler_InvalidId_SilentlyContinueNoBroadcast() {
            // Given
            var mediator = Substitute.For<IMediator>();
            var handler = new DeleteNoteGroupCommandHandler(
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

            var command = new DeleteNoteGroupCommand(retroId, -1);

            // When
            await handler.Handle(command, CancellationToken.None);

            // Then
            Assert.That(mediator.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public async Task DeleteNoteGroupCommandHandler_ValidCommand_CreatesValidObjectAndBroadcast() {
            // Given
            var securityValidator = Substitute.For<ISecurityValidator>();
            var mediatorMock = Substitute.For<IMediator>();
            var handler = new DeleteNoteGroupCommandHandler(
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

            var command = new DeleteNoteGroupCommand(retroId, noteGroup.Id);

            // When
            Assume.That(this.Context.NoteGroups.Select(x => x.Id), Does.Contain(noteGroup.Id));
            await handler.Handle(command, CancellationToken.None);

            // Then
            await securityValidator.Received().EnsureOperation(Arg.Is<Retrospective>(r => r.Id == retro.Id), SecurityOperation.AddOrUpdate, Arg.Is<NoteGroup>(g => g.Id == noteGroup.Id));

            var broadcastedDelete = mediatorMock.ReceivedCalls().FirstOrDefault()?.GetArguments()[0] as NoteLaneUpdatedNotification;

            if (broadcastedDelete == null) {
                Assert.Fail("No broadcast has gone out");
            }

            Assert.That(broadcastedDelete.LaneId, Is.EqualTo((int)KnownNoteLane.Continue));
            Assert.That(broadcastedDelete.RetroId, Is.EqualTo(command.RetroId));
            Assert.That(broadcastedDelete.GroupId, Is.EqualTo(noteGroup.Id));

            Assert.That(this.Context.NoteGroups.Select(x => x.Id), Does.Not.Contains(noteGroup.Id));
        }
    }
}
