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
                Returns((ci) => {
                    Participant p = this.Context.Participants.Last();
                    return new ValueTask<CurrentParticipantModel>(new CurrentParticipantModel(p.Id, p.Name, p.IsFacilitator));
                });
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
            var command = CastVoteCommand.ForNote(1337, VoteMutationType.Added);

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
            var command = CastVoteCommand.ForNoteGroup(1337, VoteMutationType.Added);

            // When
            TestDelegate action = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task CastVoteCommandHandler_RemoveVote_VoteForNoteNotFound_ThrowsNothing() {
            // Given
            var handler = new CastVoteCommandHandler(
                this.Context,
                this._currentParticipantService,
                Substitute.For<ISecurityValidator>(),
                Substitute.For<IMediator>(),
                this.Mapper
            );
            Retrospective retro = await this.CreateRetrospectiveWithNote();
            CastVoteCommand command = CastVoteCommand.ForNote(retro.Notes.First().Id, VoteMutationType.Removed);

            // When
            TestDelegate action = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.Nothing);
        }

        [Test]
        public async Task CastVoteCommandHandler_RemoveVote_VoteForNoteGroupNotFound_ThrowsNothing() {
            // Given
            var handler = new CastVoteCommandHandler(
                this.Context,
                this._currentParticipantService,
                Substitute.For<ISecurityValidator>(),
                Substitute.For<IMediator>(),
                this.Mapper
            );

            Retrospective retro = await this.CreateRetrospectiveWithNoteGroup();

            CastVoteCommand command = CastVoteCommand.ForNoteGroup(retro.NoteGroup.First().Id, VoteMutationType.Removed);

            // When
            TestDelegate action = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.Nothing);
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

            Retrospective retro = await this.CreateRetrospectiveWithNoteGroup();

            CastVoteCommand command = CastVoteCommand.ForNoteGroup(retro.NoteGroup.First().Id, VoteMutationType.Added);

            // When
            await handler.Handle(command, CancellationToken.None);

            // Then
            await securityValidator.Received().
                EnsureOperation(Arg.Any<Retrospective>(), SecurityOperation.AddOrUpdate, Arg.Any<NoteVote>());

            var broadcastedNote = mediatorMock.ReceivedCalls().FirstOrDefault()?.GetArguments()[0] as VoteChangeNotification;

            if (broadcastedNote == null) {
                Assert.Fail("No broadcast has gone out");
            }

            Assert.That(broadcastedNote.VoteChange.RetroId, Is.EqualTo(retro.UrlId.StringId));
            Assert.That(broadcastedNote.VoteChange.Mutation, Is.EqualTo(VoteMutationType.Added));
            Assert.That(broadcastedNote.VoteChange.Vote.NoteGroupId, Is.EqualTo(retro.NoteGroup.First().Id));
        }

        [Test]
        public async Task CastVoteCommandHandler_NoteGroupVoteRemoved_VoteSavedAndBroadcast() {
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

            Retrospective retro = await this.CreateRetrospectiveWithNoteGroup();

            NoteGroup noteGroup = retro.NoteGroup.First();

            NoteVote note = this.Context.NoteVotes.Add(new NoteVote {
                NoteGroup = noteGroup,
                Participant = retro.Participants.First(),
                Retrospective = retro
            }).Entity;
            await this.Context.SaveChangesAsync();

            CastVoteCommand command = CastVoteCommand.ForNoteGroup(noteGroup.Id, VoteMutationType.Removed);

            // When
            await handler.Handle(command, CancellationToken.None);

            // Then
            TestContext.WriteLine(securityValidator.ReceivedCalls().Select(x => x.GetMethodInfo().Name).FirstOrDefault());
            await securityValidator.Received().
                EnsureOperation(Arg.Any<Retrospective>(), SecurityOperation.Delete, Arg.Any<NoteVote>());

            var broadcastedNote = mediatorMock.ReceivedCalls().FirstOrDefault()?.GetArguments()[0] as VoteChangeNotification;

            if (broadcastedNote == null) {
                Assert.Fail("No broadcast has gone out");
            }

            Assert.That(broadcastedNote.VoteChange.RetroId, Is.EqualTo(retro.UrlId.StringId));
            Assert.That(broadcastedNote.VoteChange.Mutation, Is.EqualTo(VoteMutationType.Removed));
            Assert.That(broadcastedNote.VoteChange.Vote.NoteGroupId, Is.EqualTo(noteGroup.Id));

            Assert.That(this.Context.NoteVotes.Select(x => x.Id), Does.Not.Contain(note.Id));
        }

        private async Task<Retrospective> CreateRetrospectiveWithNoteGroup() {
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

            TestContext.WriteLine(retroId);
            return retro;
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

            Retrospective retro = await this.CreateRetrospectiveWithNote();

            Note note = retro.Notes.First();
            CastVoteCommand command = CastVoteCommand.ForNote(note.Id, VoteMutationType.Added);

            // When
            await handler.Handle(command, CancellationToken.None);

            // Then
            await securityValidator.Received().
                EnsureOperation(Arg.Any<Retrospective>(), SecurityOperation.AddOrUpdate, Arg.Any<NoteVote>());

            var broadcastedNote = mediatorMock.ReceivedCalls().FirstOrDefault()?.GetArguments()[0] as VoteChangeNotification;

            if (broadcastedNote == null) {
                Assert.Fail("No broadcast has gone out");
            }

            Assert.That(broadcastedNote.VoteChange.RetroId, Is.EqualTo(retro.UrlId.StringId));
            Assert.That(broadcastedNote.VoteChange.Mutation, Is.EqualTo(VoteMutationType.Added));
            Assert.That(broadcastedNote.VoteChange.Vote.NoteId, Is.EqualTo(note.Id));
        }

        private async Task<Retrospective> CreateRetrospectiveWithNote() {
            var retro = new Retrospective {
                CurrentStage = RetrospectiveStage.Writing,
                FacilitatorHashedPassphrase = "whatever",
                Title = TestContext.CurrentContext.Test.FullName,
                CreationTimestamp = DateTimeOffset.Now,
                Participants = { new Participant { Name = "John" } },
                Notes = { new Note { Lane = this.Context.NoteLanes.Find(KnownNoteLane.Continue), Text = "???" } }
            };
            Note note = retro.Notes.First();
            note.Participant = retro.Participants.First();

            string retroId = retro.UrlId.StringId;
            this.Context.Retrospectives.Add(retro);
            await this.Context.SaveChangesAsync();

            TestContext.WriteLine(retroId);
            return retro;
        }

        [Test]
        public async Task CastVoteCommandHandler_NoteVoteRemoved_VoteSavedAndBroadcast() {
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

            Retrospective retro = await this.CreateRetrospectiveWithNote();

            Note note = retro.Notes.First();

            NoteVote noteVote = this.Context.NoteVotes.Add(new NoteVote {
                Note = note,
                Participant = note.Participant,
                ParticipantId = note.ParticipantId,
                Retrospective = retro
            }).Entity;
            await this.Context.SaveChangesAsync();

            CastVoteCommand command = CastVoteCommand.ForNote(note.Id, VoteMutationType.Removed);

            // When
            await handler.Handle(command, CancellationToken.None);

            // Then
            await securityValidator.Received().
                EnsureOperation(Arg.Any<Retrospective>(), SecurityOperation.Delete, Arg.Any<NoteVote>());

            var broadcastedNote = mediatorMock.ReceivedCalls().FirstOrDefault()?.GetArguments()[0] as VoteChangeNotification;

            if (broadcastedNote == null) {
                Assert.Fail("No broadcast has gone out");
            }

            Assert.That(broadcastedNote.VoteChange.RetroId, Is.EqualTo(retro.UrlId.StringId));
            Assert.That(broadcastedNote.VoteChange.Mutation, Is.EqualTo(VoteMutationType.Removed));
            Assert.That(broadcastedNote.VoteChange.Vote.NoteId, Is.EqualTo(note.Id));

            Assert.That(this.Context.NoteVotes.Select(x => x.Id), Does.Not.Contain(noteVote.Id));
        }
    }
}
