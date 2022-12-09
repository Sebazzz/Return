// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveLobbyVotingTests.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Pages;

using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Components;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using NUnit.Framework;

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

        await this.WaitNavigatedToLobby();

        // When
        await this.Client1.VoteCountInput.FillAsync("2");
        await this.Client1.TimeInMinutesInput.FillAsync("10");
        await this.Client1.WorkflowContinueButton.ClickAsync();

        // Then
        await this.MultiAssert(async client => {
            const int expectedVoteCount = 2 * 3 /* Number of lanes */;
            VoteStatusPanelComponent voteStatusPanel = client.VoteStatus;
            VoteStatusForParticipant bossVoteStatus = (await voteStatusPanel.VoteStatusPerParticipant()).First(x => x.ParticipantId == bossId);

            Assert.That(() => bossVoteStatus.TotalVotes.CountAsync().GetAwaiter().GetResult(),
                Is.EqualTo(expectedVoteCount).Retry(),
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

        await this.WaitNavigatedToLobby();

        // When
        await this.Client1.VoteCountInput.FillAsync("2");
        await this.Client1.TimeInMinutesInput.FillAsync("10");
        await this.Client1.WorkflowContinueButton.ClickAsync();

        var allLanes = new[] { KnownNoteLane.Start, KnownNoteLane.Stop, KnownNoteLane.Continue };
        foreach (KnownNoteLane noteLaneId in allLanes) {
            NoteLaneComponent noteLane = this.Client2.GetLane(noteLaneId);

            foreach (VoteListComponent voteListComponent in await noteLane.VoteLists()) {
                for (int cnt = 0; cnt < 2; cnt++) {
                    await voteListComponent.ClickVoteButton();
                }

                break;
            }
        }

        // Then
        await this.MultiAssert(async client => {
            const int expectedVoteCount = 2 * 3 /* Number of lanes */;

            async Task AssertVoteCountAsync(int pid, int count, Func<VoteStatusForParticipant, ILocator> voteListSelector) {
                VoteStatusPanelComponent voteStatusPanel = client.VoteStatus;

                ILocator locator = voteStatusPanel.VoteStatusPerParticipantLocator(pid);
                await locator.Expected().ToBeVisibleAsync();

                VoteStatusForParticipant participantVoteStatus = await VoteStatusForParticipant.Create(locator);

                await voteListSelector(participantVoteStatus).Expected().ToHaveCountAsync(count);
            }

            void AssertVoteCount(int pid, int count, Func<VoteStatusForParticipant, ILocator> voteListSelector) {
                Assert.DoesNotThrowAsync(() => AssertVoteCountAsync(pid,count,voteListSelector), $"Either unable to find vote panel, unable to find vote status for participant #{pid}, or the vote count is incorrect (not {count})");
            }

            AssertVoteCount(participantId, expectedVoteCount, v => v.CastVotes);
            AssertVoteCount(bossId, expectedVoteCount, v => v.UncastVotes);
            AssertVoteCount(bossId, 0, v => v.CastVotes);
            AssertVoteCount(participantId, 0, v => v.UncastVotes);

            foreach (KnownNoteLane noteLaneId in allLanes) {
                NoteLaneComponent noteLane = client.GetLane(noteLaneId);

                foreach (VoteListComponent voteListComponent in await noteLane.VoteLists())
                {
                    await voteListComponent.Votes.Expected().ToHaveCountAsync(2);

                    if (client == this.Client2)
                    {
                        await voteListComponent.IsVoteButtonEnabled(false);
                    }
                    else {
                        await voteListComponent.IsVoteButtonEnabled(true);
                    }

                    break;
                }
            }
        });
    }
}
