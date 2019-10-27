// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveLobbyVotingTests.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Pages {
    using System;
    using System.Collections;
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using Components;
    using Domain.Entities;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using OpenQA.Selenium;

    [TestFixture]
    public sealed class RetrospectiveLobbyVotingTests : RetrospectiveLobbyTestsBase {
        [SetUp]
        public async Task SetUp() {
            using IServiceScope scope = this.App.CreateTestServiceScope();
            this.RetroId = await scope.CreateRetrospective("scrummaster");
        }

        [Test]
        public async Task RetrospectiveLobby_ShowsCorrectVoteCount_OnStartingVotes() {
            // Given
            int bossId = 0;
            using (IServiceScope scope = this.App.CreateTestServiceScope()) {
                await scope.TestCaseBuilder(this.RetroId).
                    WithParticipant("Boss", true, "scrummaster").
                    OutputId(id => bossId = id).
                    WithParticipant("Josh", false).
                    WithRetrospectiveStage(RetrospectiveStage.Writing).
                    WithNote(KnownNoteLane.Start, "Josh").
                    WithNote(KnownNoteLane.Continue, "Josh").
                    WithNote(KnownNoteLane.Continue, "Boss").
                    WithNote(KnownNoteLane.Continue, "Boss").
                    WithRetrospectiveStage(RetrospectiveStage.Grouping).
                    Build();
            }

            await Task.WhenAll(
                Task.Run(() => this.Join(this.Client1, true, "Boss", alreadyJoined: true)),
                Task.Run(() => this.Join(this.Client2, false, "Josh", true))
            );

            this.WaitNavigatedToLobby();

            // When
            this.Client1.VoteCountInput.SendKeys("2");
            this.Client1.TimeInMinutesInput.SendKeys("10");
            this.Client1.WorkflowContinueButton.Click();

            // Then
            this.MultiAssert(client => {
                const int expectedVoteCount = 2 * 3 /* Number of lanes */;
                Assert.That(() => {
                    VoteStatusPanelComponent voteStatusPanel = client.VoteStatus;
                    VoteStatusForParticipant bossVoteStatus =
                        voteStatusPanel.VoteStatusPerParticipant.First(x => x.ParticipantId == bossId);
                    return bossVoteStatus.TotalVotes;
                },
                    Has.Count.EqualTo(expectedVoteCount).Retry(),
                    $"Either unable to find vote panel, unable to find vote status for participant #{bossId}, or the vote count is incorrect (not {expectedVoteCount})");
            });
        }

        [Test]
        public async Task RetrospectiveLobby_ShowsUpdatesVotes_OnVoting() {
            // Given
            int bossId = 0;
            int participantId = 0;
            using (IServiceScope scope = this.App.CreateTestServiceScope()) {
                await scope.TestCaseBuilder(this.RetroId).
                    WithParticipant("Boss", true, "scrummaster").
                    OutputId(id => bossId = id).
                    WithParticipant("Josh", false).
                    OutputId(id => participantId = id).
                    WithRetrospectiveStage(RetrospectiveStage.Writing).
                    WithNote(KnownNoteLane.Start, "Josh").
                    WithNote(KnownNoteLane.Continue, "Josh").
                    WithNote(KnownNoteLane.Continue, "Boss").
                    WithNote(KnownNoteLane.Continue, "Boss").
                    WithNote(KnownNoteLane.Stop, "Boss").
                    WithRetrospectiveStage(RetrospectiveStage.Grouping).
                    Build();
            }

            await Task.WhenAll(
                Task.Run(() => this.Join(this.Client1, true, "Boss", alreadyJoined: true)),
                Task.Run(() => this.Join(this.Client2, false, "Josh", true))
            );

            this.WaitNavigatedToLobby();

            // When
            this.Client1.VoteCountInput.SendKeys("2");
            this.Client1.TimeInMinutesInput.SendKeys("10");
            this.Client1.WorkflowContinueButton.Click();

            var allLanes = new[] { KnownNoteLane.Start, KnownNoteLane.Stop, KnownNoteLane.Continue };
            foreach (KnownNoteLane noteLaneId in allLanes) {
                NoteLaneComponent noteLane = this.Client2.GetLane(noteLaneId);

                foreach (VoteListComponent voteListComponent in noteLane.VoteLists) {
                    for (int cnt = 0; cnt < 2; cnt++) {
                        voteListComponent.ClickVoteButton();
                    }

                    break;
                }
            }

            // Then
            this.MultiAssert(client => {
                const int expectedVoteCount = 2 * 3 /* Number of lanes */;

                void AssertVoteCount(int pid, int count, Func<VoteStatusForParticipant, IEnumerable> voteListSelector) {
                    Assert.That(() => {
                        VoteStatusPanelComponent voteStatusPanel = client.VoteStatus;
                        VoteStatusForParticipant bossVoteStatus =
                            voteStatusPanel.VoteStatusPerParticipant.First(x => x.ParticipantId == pid);
                        return voteListSelector(bossVoteStatus);
                    },
                        Has.Count.EqualTo(count).Retry(),
                        $"Either unable to find vote panel, unable to find vote status for participant #{pid}, or the vote count is incorrect (not {count})");
                }

                AssertVoteCount(participantId, expectedVoteCount, v => v.CastVotes);
                AssertVoteCount(bossId, expectedVoteCount, v => v.UncastVotes);
                AssertVoteCount(bossId, 0, v => v.CastVotes);
                AssertVoteCount(participantId, 0, v => v.UncastVotes);

                foreach (KnownNoteLane noteLaneId in allLanes) {
                    NoteLaneComponent noteLane = client.GetLane(noteLaneId);

                    foreach (VoteListComponent voteListComponent in noteLane.VoteLists) {
                        Assert.That(() => voteListComponent.Votes, Has.Count.EqualTo(2),
                            "Expected to find 2 votes in each lane");

                        if (client == this.Client2) {
                            Assert.That(() => voteListComponent.IsVoteButtonEnabled, Is.False, "Vote button should be disabled");
                        }
                        else {
                            Assert.That(() => voteListComponent.IsVoteButtonEnabled, Is.True, "Vote button should be enabled");
                        }

                        break;
                    }
                }
            });
        }
    }
}
