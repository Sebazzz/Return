// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : DeleteNoteCommandHandlerTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Notes.Commands {
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common;
    using Application.Common.Abstractions;
    using Application.Common.Models;
    using Application.Common.Security;
    using Application.Notes.Commands.DeleteNote;
    using Application.Notifications.NoteAdded;
    using Application.Notifications.NoteDeleted;
    using Domain.Entities;
    using MediatR;
    using NSubstitute;
    using NUnit.Framework;
    using Return.Common;
    using Support;

    [TestFixture]
    public sealed class DeleteNoteCommandHandlerTests : QueryTestBase {
        private readonly ICurrentParticipantService _currentParticipantService;
        private readonly ISystemClock _systemClock;

        public DeleteNoteCommandHandlerTests() {
            this._currentParticipantService = Substitute.For<ICurrentParticipantService>();
            this._currentParticipantService.GetParticipant().
                Returns(new ValueTask<CurrentParticipantModel>(new CurrentParticipantModel(1, "Derp", false)));

            this._systemClock = Substitute.For<ISystemClock>();
            this._systemClock.CurrentTimeOffset.Returns(DateTimeOffset.UnixEpoch);
        }

        [Test]
        public void DeleteNoteCommandHandler_InvalidRetroId_ThrowsNotFoundException() {
            // Given
            var handler = new DeleteNoteCommandHandler(
                this.Context,
                Substitute.For<ISecurityValidator>(),
                Substitute.For<IMediator>()
                );
            var command = new DeleteNoteCommand("not found", (int)KnownNoteLane.Start);

            // When
            TestDelegate action = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task DeleteNoteCommandHandler_InvalidLaneId_ThrowsNotFoundException() {
            // Given
            var handler = new DeleteNoteCommandHandler(
                this.Context,
                Substitute.For<ISecurityValidator>(),
                Substitute.For<IMediator>()
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

            var command = new DeleteNoteCommand(retroId, -1);

            // When
            TestDelegate action = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task DeleteNoteCommandHandler_ValidCommand_CreatesValidObjectAndBroadcast() {
            // Given
            var securityValidator = Substitute.For<ISecurityValidator>();
            var mediatorMock = Substitute.For<IMediator>();
            var handler = new DeleteNoteCommandHandler(
                this.Context,
                securityValidator,
                mediatorMock
            );

            var note = new Note {
                Retrospective = new Retrospective {
                    FacilitatorHashedPassphrase = "whatever",
                    CurrentStage = RetrospectiveStage.Writing,
                    Title = this.GetType().FullName
                },
                Participant = new Participant {
                    Name = "Tester"
                },
                Lane = this.Context.NoteLanes.FirstOrDefault(),
                Text = "Derp"
            };

            TestContext.WriteLine(note.Retrospective.UrlId);
            this.Context.Notes.Add(note);
            await this.Context.SaveChangesAsync();

            var command = new DeleteNoteCommand(note.Retrospective.UrlId.StringId, note.Id);

            // When
            await handler.Handle(command, CancellationToken.None);

            // Then
            await securityValidator.Received().EnsureOperation(Arg.Is<Retrospective>(r => r.Id == note.Retrospective.Id), SecurityOperation.Delete, Arg.Any<Note>());

            var broadcastedNote = mediatorMock.ReceivedCalls().FirstOrDefault()?.GetArguments()[0] as NoteDeletedNotification;

            if (broadcastedNote == null) {
                Assert.Fail("No broadcast has gone out");
            }

            Assert.That(broadcastedNote.RetroId, Is.EqualTo(command.RetroId));
            Assert.That(broadcastedNote.LaneId, Is.EqualTo((int?)note.Lane?.Id));
            Assert.That(broadcastedNote.NoteId, Is.EqualTo(note.Id));

        }
    }
}
