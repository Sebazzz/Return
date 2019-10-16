// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveVoteStatusTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Votes.Queries {
    using System.Linq;
    using Application.Common.Models;
    using Application.Notifications.VoteChanged;
    using Application.Votes.Queries;
    using NUnit.Framework;

    [TestFixture]
    public static class RetrospectiveVoteStatusTests {
        [Test]
        public static void RetrospectiveVoteStatus_AddVote_AddsVote() {
            // Given
            var voteStatus = new RetrospectiveVoteStatus(3);
            voteStatus.Votes.Add(new VoteModel { ParticipantId = 1, NoteId = 2, LaneId = 3 });
            voteStatus.Votes.Add(new VoteModel { ParticipantId = 1 });
            voteStatus.Initialize();

            Assume.That(voteStatus.Votes.Count(x => x.IsCast == false), Is.EqualTo(1));

            // When
            voteStatus.Apply(new VoteChange("not-relevant", new VoteModel {
                ParticipantId = 1,
                NoteGroupId = 3
            }, VoteMutationType.Added));

            // Then
            Assert.That(voteStatus.Votes.Count(x => x.IsCast == false), Is.EqualTo(0));
        }

        [Test]
        public static void RetrospectiveVoteStatus_RemoveVote_RemovesVote() {
            // Given
            var voteStatus = new RetrospectiveVoteStatus(3);
            voteStatus.Votes.Add(new VoteModel { Id = 3, ParticipantId = 1, NoteId = 2, LaneId = 3 });
            voteStatus.Votes.Add(new VoteModel { Id = 4, ParticipantId = 1 });
            voteStatus.Initialize();

            Assume.That(voteStatus.Votes.Count(x => x.IsCast == false), Is.EqualTo(1));

            // When
            voteStatus.Apply(new VoteChange("not-relevant", new VoteModel {
                Id = 3,
                ParticipantId = 1,
                NoteId = 2
            }, VoteMutationType.Removed));

            // Then
            Assert.That(voteStatus.Votes.Count(x => x.IsCast == false), Is.EqualTo(2));
        }
    }
}
