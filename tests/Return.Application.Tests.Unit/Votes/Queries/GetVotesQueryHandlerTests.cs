// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetVotesQueryHandlerTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Votes.Queries {
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common;
    using Application.Votes.Queries;
    using Domain.Entities;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class GetVotesQueryHandlerTests : QueryTestBase {
        private string _retroId = "";
        private int _note1Id;
        private int _note2Id;
        private int _noteGroupId;
        private int _participant1Id;
        private int _participant2Id;

        [SetUp]
        public async Task SetUp() {
            var p1 = new Participant {
                Name = "Tester 1"
            };
            var p2 = new Participant {
                Name = "Tester 2"
            };

            var retro = new Retrospective {
                FacilitatorHashedPassphrase = "whatever",
                CurrentStage = RetrospectiveStage.Voting,
                Title = this.GetType().FullName,
                Participants = { p1, p2 },
                Options =
                {
                    MaximumNumberOfVotes = 3
                }
            };
            this._retroId = retro.UrlId.StringId;
            this.Context.Retrospectives.Add(retro);

            Note note1 = this.Context.Notes.Add(new Note {
                Retrospective = retro,
                Participant = p1,
                Lane = this.Context.NoteLanes.FirstOrDefault(),
                Text = "Test 1"
            }).Entity;
            Note note2 = this.Context.Notes.Add(new Note {
                Retrospective = retro,
                Participant = p1,
                Lane = this.Context.NoteLanes.FirstOrDefault(),
                Text = "Test 2"
            }).Entity;

            var noteGroup1 = new NoteGroup {
                Title = "Group",
                Retrospective = retro,
                Lane = this.Context.NoteLanes.FirstOrDefault()
            };
            this.Context.NoteGroups.Add(noteGroup1);
            this.Context.Notes.Add(new Note {
                Retrospective = retro,
                Participant = p2,
                Group = noteGroup1,
                Lane = this.Context.NoteLanes.FirstOrDefault(),
                Text = "Test 3"
            });

            await this.Context.SaveChangesAsync();

            this.Context.NoteVotes.Add(new NoteVote {
                Note = note1,
                Participant = p1,
                Retrospective = retro
            });
            this.Context.NoteVotes.Add(new NoteVote {
                Note = note1,
                Participant = p1,
                Retrospective = retro
            });
            this.Context.NoteVotes.Add(new NoteVote {
                Note = note2,
                Participant = p1,
                Retrospective = retro
            });
            this.Context.NoteVotes.Add(new NoteVote {
                Note = note2,
                Participant = p2,
                Retrospective = retro
            });
            this.Context.NoteVotes.Add(new NoteVote {
                NoteGroup = noteGroup1,
                Participant = p2,
                Retrospective = retro
            });

            await this.Context.SaveChangesAsync();

            this._note1Id = note1.Id;
            this._note2Id = note2.Id;
            this._noteGroupId = noteGroup1.Id;
            this._participant1Id = p1.Id;
            this._participant2Id = p2.Id;

            // We have now:
            // - A retrospective
            // - Two notes from p1
            // - One note in a group from p2
            // Note 1 has two votes from p1
            // Note 2 has note group has vote from p2
            // Note 2 has vote from p1
        }

        [Test]
        public void GetVotesQueryHandlerTests_ThrowsNotFoundException_WhenNotFound() {
            // Given
            const string retroId = "surely-not-found";
            var query = new GetVotesQuery(retroId);
            var handler = new GetVotesQueryHandler(this.Context, this.Mapper);

            // When
            TestDelegate action = () => handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task GetVotesQueryHandlerTests_ReturnsVotesPerParticipant_BasedOnRetrospectiveOptions() {
            // Given
            var command = new GetVotesQuery(this._retroId);
            var handler = new GetVotesQueryHandler(this.Context, this.Mapper);

            int laneCount = this.Context.NoteLanes.Count();
            int votePerLaneCount = this.Context.Retrospectives.Where(x => x.UrlId.StringId == this._retroId).
                Select(x => x.Options.MaximumNumberOfVotes).
                First();
            int votesPerParticipant = laneCount * votePerLaneCount;

            // When
            RetrospectiveVoteStatus result = (await handler.Handle(command, CancellationToken.None)).VoteStatus;

            // Then
            Assert.That(result.Votes, Has.Count.EqualTo(2 * votesPerParticipant));
            Assert.That(result.VotesByParticipant.Get(this._participant1Id), Has.Count.EqualTo(votesPerParticipant));
            Assert.That(result.VotesByParticipant.Get(this._participant2Id), Has.Count.EqualTo(votesPerParticipant));
            Assert.That(result.VotesByNote.Get(this._note1Id).Select(x => x.ParticipantId),
                Is.EquivalentTo(new[] { this._participant1Id, this._participant1Id }));
            Assert.That(result.VotesByNote.Get(this._note2Id).Select(x => x.ParticipantId),
                Is.EquivalentTo(new[] { this._participant1Id, this._participant2Id }));
            Assert.That(result.VotesByNoteGroup.Get(this._noteGroupId).Select(x => x.ParticipantId),
                Is.EquivalentTo(new[] { this._participant2Id }));
        }
    }
}
