// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : AddNoteCommandHandlerTests.cs
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
    using Application.Notes.Commands.AddNote;
    using Application.Notifications.NoteAdded;
    using Domain.Entities;
    using MediatR;
    using NSubstitute;
    using NUnit.Framework;
    using Return.Common;
    using Support;

    [TestFixture]
    public sealed class AddNoteCommandHandlerTests : QueryTestBase {
        private readonly ICurrentParticipantService _currentParticipantService;
        private readonly ISystemClock _systemClock;

        public AddNoteCommandHandlerTests() {
            this._currentParticipantService = Substitute.For<ICurrentParticipantService>();
            this._currentParticipantService.GetParticipant().
                Returns(new ValueTask<CurrentParticipantModel>(new CurrentParticipantModel(1, "Derp", false)));

            this._systemClock = Substitute.For<ISystemClock>();
            this._systemClock.CurrentTimeOffset.Returns(DateTimeOffset.UnixEpoch);
        }

        [Test]
        public void AddNoteCommandHandler_InvalidRetroId_ThrowsNotFoundException() {
            // Given
            var handler = new AddNoteCommandHandler(
                this.Context,
                this._currentParticipantService,
                Substitute.For<ISecurityValidator>(),
                Substitute.For<IMediator>(),
                this.Mapper,
                this._systemClock
                );
            var command = new AddNoteCommand("not found", (int)KnownNoteLane.Start);

            // When
            TestDelegate action = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task AddNoteCommandHandler_InvalidLaneId_ThrowsNotFoundException() {
            // Given
            var handler = new AddNoteCommandHandler(
                this.Context,
                this._currentParticipantService,
                Substitute.For<ISecurityValidator>(),
                Substitute.For<IMediator>(),
                this.Mapper,
                this._systemClock
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

            var command = new AddNoteCommand(retroId, -1);

            // When
            TestDelegate action = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task AddNoteCommandHandler_ValidCommand_CreatesValidObjectAndBroadcast() {
            // Given
            var securityValidator = Substitute.For<ISecurityValidator>();
            var mediatorMock = Substitute.For<IMediator>();
            var handler = new AddNoteCommandHandler(
                this.Context,
                this._currentParticipantService,
                securityValidator,
                mediatorMock,
                this.Mapper,
                this._systemClock
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

            var command = new AddNoteCommand(retroId, (int)KnownNoteLane.Start);

            // When
            RetrospectiveNote result = await handler.Handle(command, CancellationToken.None);

            // Then
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ParticipantName, Is.EqualTo(retro.Participants.First().Name));
            Assert.That(result.IsOwnedByCurrentUser, Is.True);

            await securityValidator.Received().EnsureOperation(Arg.Is<Retrospective>(r => r.Id == retro.Id), SecurityOperation.AddOrUpdate, Arg.Any<Note>());

            var broadcastedNote = mediatorMock.ReceivedCalls().FirstOrDefault()?.GetArguments()[0] as NoteAddedNotification;

            if (broadcastedNote == null) {
                Assert.Fail("No broadcast has gone out");
            }

            Assert.That(broadcastedNote.LaneId, Is.EqualTo((int)KnownNoteLane.Start));
            Assert.That(broadcastedNote.RetroId, Is.EqualTo(command.RetroId));
            Assert.That(broadcastedNote.Note.IsOwnedByCurrentUser, Is.False);
        }
    }
}
