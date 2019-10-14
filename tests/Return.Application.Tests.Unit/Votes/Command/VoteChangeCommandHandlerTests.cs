// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : VoteChangeCommandHandlerTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Votes.Command {
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common;
    using Application.Common.Abstractions;
    using Application.Common.Models;
    using Application.Common.Security;
    using Application.Notifications.VoteChanged;
    using Application.Votes.Commands;
    using Domain.Entities;
    using MediatR;
    using NSubstitute;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class CastVoteCommandHandlerTests : QueryTestBase {
        private readonly ICurrentParticipantService _currentParticipantService;

        public CastVoteCommandHandlerTests() {
            this._currentParticipantService = Substitute.For<ICurrentParticipantService>();
            this._currentParticipantService.GetParticipant().
                Returns(new ValueTask<CurrentParticipantModel>(new CurrentParticipantModel(1, "Derp", false)));
        }

        [Test]
        public void CastVoteCommandHandler_NoteNotFound_ThrowsException() {
            // Given
            var handler = new CastVoteCommandHandler(
                this.Context,
                Substitute.For<ICurrentParticipantService>(),
                Substitute.For<ISecurityValidator>(),
                Substitute.For<IMediator>(),
                this.Mapper
            );
            var command = CastVoteCommand.ForNote(1337);

            // When
            TestDelegate action = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public void CastVoteCommandHandler_NoteGroupNotFound_ThrowsException() {
            // Given
            var handler = new CastVoteCommandHandler(
                this.Context,
                Substitute.For<ICurrentParticipantService>(),
                Substitute.For<ISecurityValidator>(),
                Substitute.For<IMediator>(),
                this.Mapper
            );
            var command = CastVoteCommand.ForNoteGroup(1337);

            // When
            TestDelegate action = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task CastVoteCommandHandler_NoteGroupVote_VoteSavedAndBroadcast() {
            // Given
            var securityValidator = Substitute.For<ISecurityValidator>();
            var mediatorMock = Substitute.For<IMediator>();
            var handler = new CastVoteCommandHandler(
                this.Context,
                this._currentParticipantService,
                securityValidator,
                mediatorMock,
                this.Mapper
            );

            var retro = new Retrospective {
                CurrentStage = RetrospectiveStage.Writing,
                ManagerHashedPassphrase = "whatever",
                Title = TestContext.CurrentContext.Test.FullName,
                CreationTimestamp = DateTimeOffset.Now,
                Participants = { new Participant { Name = "John" } },
                NoteGroup = { new NoteGroup { Lane = this.Context.NoteLanes.Find(KnownNoteLane.Continue), Title = "???" } }
            };
            string retroId = retro.UrlId.StringId;
            this.Context.Retrospectives.Add(retro);
            await this.Context.SaveChangesAsync();

            TestContext.WriteLine(retroId);

            var command = CastVoteCommand.ForNoteGroup(retro.NoteGroup.First().Id);

            // When
            await handler.Handle(command, CancellationToken.None);

            // Then
            await securityValidator.Received().
                EnsureOperation(Arg.Any<Retrospective>(), SecurityOperation.AddOrUpdate, Arg.Any<NoteVote>());

            var broadcastedNote = mediatorMock.ReceivedCalls().FirstOrDefault()?.GetArguments()[0] as VoteChangeNotification;

            if (broadcastedNote == null) {
                Assert.Fail("No broadcast has gone out");
            }

            Assert.That(broadcastedNote.VoteChange.RetroId, Is.EqualTo(retroId));
            Assert.That(broadcastedNote.VoteChange.Mutation, Is.EqualTo(VoteMutationType.Added));
            Assert.That(broadcastedNote.VoteChange.Vote.NoteGroupId, Is.EqualTo(retro.NoteGroup.First().Id));
        }

        [Test]
        public async Task CastVoteCommandHandler_NoteVote_VoteSavedAndBroadcast() {
            // Given
            var securityValidator = Substitute.For<ISecurityValidator>();
            var mediatorMock = Substitute.For<IMediator>();
            var handler = new CastVoteCommandHandler(
                this.Context,
                this._currentParticipantService,
                securityValidator,
                mediatorMock,
                this.Mapper
            );

            var retro = new Retrospective {
                CurrentStage = RetrospectiveStage.Writing,
                ManagerHashedPassphrase = "whatever",
                Title = TestContext.CurrentContext.Test.FullName,
                CreationTimestamp = DateTimeOffset.Now,
                Participants = { new Participant { Name = "John" } },
                Notes = { new Note { Lane = this.Context.NoteLanes.Find(KnownNoteLane.Continue), Text = "???" } }
            };
            var note = retro.Notes.First();
            note.Participant = retro.Participants.First();

            string retroId = retro.UrlId.StringId;
            this.Context.Retrospectives.Add(retro);
            await this.Context.SaveChangesAsync();

            TestContext.WriteLine(retroId);

            var command = CastVoteCommand.ForNote(note.Id);

            // When
            await handler.Handle(command, CancellationToken.None);

            // Then
            await securityValidator.Received().
                EnsureOperation(Arg.Any<Retrospective>(), SecurityOperation.AddOrUpdate, Arg.Any<NoteVote>());

            var broadcastedNote = mediatorMock.ReceivedCalls().FirstOrDefault()?.GetArguments()[0] as VoteChangeNotification;

            if (broadcastedNote == null) {
                Assert.Fail("No broadcast has gone out");
            }

            Assert.That(broadcastedNote.VoteChange.RetroId, Is.EqualTo(retroId));
            Assert.That(broadcastedNote.VoteChange.Mutation, Is.EqualTo(VoteMutationType.Added));
            Assert.That(broadcastedNote.VoteChange.Vote.NoteId, Is.EqualTo(note.Id));
        }
    }
}
