// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ScreenshotTests.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Pages;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Abstractions;
using Application.Services;
using Common;
using Components;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using NUnit.Framework;

/// <summary>
/// Not really a real test, but more because I'm tired of creating screenshots. Note these tests don't really
/// run independently, and they are ordered by the [Order] attribute. 
/// </summary>
/// <remarks>
/// Client1: Roger (facilitator)
/// Client2: Hong (participant)
/// </remarks>
[TestFixture]
public sealed class ScreenshotTests : RetrospectiveLobbyTestsBase {
    private readonly HashSet<int> _startAutomationGroupNoteIds = new();
    private readonly HashSet<int> _stopBacklogUnstableNoteIds = new();
    private readonly HashSet<int> _stopSprintScopeIncreasedNoteIds = new();
    private readonly HashSet<int> _stopKickOffLongNoteIds = new();
    private readonly HashSet<int> _continueDailiesNoteIds = new();

    private int _startAutomationGroupId;
    private int _stopKickOffLongGroupId;
    private int _stopSprintScopeIncreasedGroupId;
    private int _stopBacklogUnstableNoteGroupId;
    private int _continueDailiesNoteGroupId;
    private int _startNoteDefinitionOfDoneId;
    private int _continueFrameworkNoteId;
    private int _startClientRetroPresenceNoteId;
    private int _continueDailyBuildNoteId;

    [Test]
    [Order((int)RetrospectiveStage.NotStarted)]
    public async Task Screenshot_CreateRetrospective() {
        // Given
        using var createRetroPage = new CreateRetrospectivePage();
        createRetroPage.InitializeFrom(this.Client1);
        await createRetroPage.Navigate(this.App);

        Task SetResolution(IPage webDriver) => webDriver.SetViewportSizeAsync(1280, 1024);

        await SetResolution(this.Client1.BrowserPage);
        await SetResolution(this.Client2.BrowserPage);

        // When
        await createRetroPage.RetrospectiveTitleInput.TypeAsync("Sprint 1: Initial prototype");
        await createRetroPage.FacilitatorPassphraseInput.TypeAsync("scrummaster");
        await createRetroPage.ParticipantPassphraseInput.TypeAsync("secret");

        // Then
        await CreateDocScreenshot(createRetroPage.BrowserPage, "create-retro");

        await createRetroPage.Submit();

        Regex regex = new("/retrospective/(?<retroId>[A-z0-9]+)/join", RegexOptions.IgnoreCase);

        await createRetroPage.UrlLocationInput.Expected().ToHaveValueAsync(regex);

        string url = await createRetroPage.UrlLocationInput.InputValueAsync();
        string retroId = regex.Match(url).
            Groups["retroId"].
            Value;
        this.RetroId = retroId;
    }

    [Test]
    [Order((int)RetrospectiveStage.Writing)]
    public async Task Screenshot_Writing() {
        this.EnsureRetrospectiveInStage(RetrospectiveStage.NotStarted);

        // Given
        using (IServiceScope scope = this.App.CreateTestServiceScope()) {
            await scope.SetRetrospective(this.RetroId, r => r.HashedPassphrase = null);
        }

        await this.Join(this.Client1, true, "Roger", colorName: "Driver red", submitCallback: () => CreateDocScreenshot(this.Client1.BrowserPage, "join-retro"));
        await this.Join(this.Client2, false, "Hong", colorName: "Amiable green");

        await this.WaitNavigatedToLobby();

        await this.Client1.TimeInMinutesInput.ClearAsync();
        await this.Client1.TimeInMinutesInput.FillAsync("5");
        await this.Client1.Unfocus();

        Thread.Sleep(10000);
        await this.Client1.InvokeContinueWorkflow();

        // When
        TestContext.WriteLine("Attempting to find Note Lane button after state transition");
        Thread.Sleep(10000);
        await this.Client2.GetLane(KnownNoteLane.Continue).AddNoteButton.Expected().ToBeVisibleAsync();

        async Task WriteNote(RetrospectiveLobby client, KnownNoteLane laneId, string text) {
            NoteLaneComponent lane = client.GetLane(laneId);

            await lane.AddNoteButton.ClickAsync();

            await lane.NoteElements.First.Expected().ToBeVisibleAsync();
            NoteComponent addedNote = (await lane.Notes()).First();
            await addedNote.Input.FillAsync(text);
        }

        await WriteNote(this.Client2, KnownNoteLane.Continue, "Using this framework, it works very productive");

        using (IServiceScope scope = this.App.CreateTestServiceScope()) {
            await scope.TestCaseBuilder(this.RetroId).
                HasExistingParticipant("Roger").
                HasExistingParticipant("Hong").
                WithParticipant("Aaron", false).
                WithParticipant("Ashley", false).
                WithParticipant("Josh", false).
                WithParticipant("Patrick", false).
                WithParticipant("Sarah", false).
                WithNote(KnownNoteLane.Continue, "Sarah", text: "Framework with good productivity").
                OutputId(id => this._continueFrameworkNoteId = id).
                WithNote(KnownNoteLane.Continue, "Josh", text: "Daily standup").
                OutputId(id => this._continueDailiesNoteIds.Add(id)).
                WithNote(KnownNoteLane.Continue, "Patrick", text: "Daily standups").
                OutputId(id => this._continueDailiesNoteIds.Add(id)).
                WithNote(KnownNoteLane.Continue, "Ashley", text: "Dailies").
                OutputId(id => this._continueDailiesNoteIds.Add(id)).
                WithNote(KnownNoteLane.Stop, "Patrick", text: "Changing the backlog in the sprint").
                OutputId(id => this._stopBacklogUnstableNoteIds.Add(id)).
                WithNote(KnownNoteLane.Stop, "Aaron", text: "Backlog story changes in sprint").
                OutputId(id => this._stopBacklogUnstableNoteIds.Add(id)).
                WithNote(KnownNoteLane.Stop, "Ashley", text: "Story backlog changes while sprint in progress").
                OutputId(id => this._stopBacklogUnstableNoteIds.Add(id)).
                WithNote(KnownNoteLane.Stop, "Ashley", text: "Adding more stories while sprint is in progress").
                OutputId(id => this._stopSprintScopeIncreasedNoteIds.Add(id)).
                WithNote(KnownNoteLane.Stop, "Josh", text: "Stop adding stories when sprint is underway").
                OutputId(id => this._stopSprintScopeIncreasedNoteIds.Add(id)).
                WithNote(KnownNoteLane.Stop, "Josh", text: "Long kick-off, distractions in sprint").
                OutputId(id => this._stopKickOffLongNoteIds.Add(id)).
                WithNote(KnownNoteLane.Stop, "Sarah", text: "Kick-off was very long").
                OutputId(id => this._stopKickOffLongNoteIds.Add(id)).
                WithNote(KnownNoteLane.Start, "Josh", text: "Start writing automated tests").
                OutputId(id => this._startAutomationGroupNoteIds.Add(id)).
                WithNote(KnownNoteLane.Start, "Patrick", text: "Use a proper definition of done").
                OutputId(id => this._startNoteDefinitionOfDoneId = id).
                WithNote(KnownNoteLane.Start, "Sarah", text: "Daily build should include basic tests").
                OutputId(id => this._startAutomationGroupNoteIds.Add(id)).
                Build();
        }

        await WriteNote(this.Client1, KnownNoteLane.Start, "Daily builds should include automated smoke tests");
        this.AddLatestNoteIdToCollection(this._startAutomationGroupNoteIds);

        await WriteNote(this.Client1, KnownNoteLane.Start, "Client should be present in retrospective");
        this._startClientRetroPresenceNoteId = this.GetLatestNoteId();

        await WriteNote(this.Client2, KnownNoteLane.Continue, "Regular publish to acceptance environment");
        this._continueDailyBuildNoteId = this.GetLatestNoteId();

        await CreateDocScreenshot(this.Client2.BrowserPage, "writing");

        // Then
        await this.Client1.InvokeContinueWorkflow();
        this.EnsureRetrospectiveInStage(RetrospectiveStage.Discuss);
    }

    [Test]
    [Order((int)RetrospectiveStage.Grouping)]
    public async Task Screenshot_Grouping() {
        this.EnsureRetrospectiveInStage(RetrospectiveStage.Discuss);

        // Given
        await this.Client1.InvokeContinueWorkflow();
        this.EnsureRetrospectiveInStage(RetrospectiveStage.Grouping);

        // When
        using (IServiceScope scope = this.App.CreateTestServiceScope()) {
            void AddNotesToGroup(TestCaseBuilder builder, int groupId, IEnumerable<int> noteIds) {
                foreach (int noteId in noteIds) {
                    builder.AddNoteToNoteGroup("Roger", noteId, groupId);
                }
            }

            await scope.TestCaseBuilder(this.RetroId).
                HasExistingParticipant("Roger").
                WithNoteGroup("Roger", KnownNoteLane.Start, "Automated tests").
                OutputId(id => this._startAutomationGroupId = id).
                WithNoteGroup("Roger", KnownNoteLane.Stop, "Kick-off long").
                OutputId(id => this._stopKickOffLongGroupId = id).
                WithNoteGroup("Roger", KnownNoteLane.Stop, "Sprint scope increased").
                OutputId(id => this._stopSprintScopeIncreasedGroupId = id).
                WithNoteGroup("Roger", KnownNoteLane.Stop, "Backlog unstable").
                OutputId(id => this._stopBacklogUnstableNoteGroupId = id).
                WithNoteGroup("Roger", KnownNoteLane.Continue, "Daily standups").
                OutputId(id => this._continueDailiesNoteGroupId = id).
                Callback(builder => {
                    AddNotesToGroup(builder, this._startAutomationGroupId, this._startAutomationGroupNoteIds);
                    AddNotesToGroup(builder,
                        this._stopBacklogUnstableNoteGroupId,
                        this._stopBacklogUnstableNoteIds);
                    AddNotesToGroup(builder,
                        this._stopSprintScopeIncreasedGroupId,
                        this._stopSprintScopeIncreasedNoteIds);
                    AddNotesToGroup(builder, this._stopKickOffLongGroupId, this._stopKickOffLongNoteIds);
                    AddNotesToGroup(builder, this._continueDailiesNoteGroupId, this._continueDailiesNoteIds);
                }).
                Build();
        }

        // Then
        await this.Client1.TimeInMinutesInput.FillAsync("4");
        await this.Client1.VoteCountInput.FillAsync("3");

        await CreateDocScreenshot(this.Client1.BrowserPage, "grouping");
    }

    [Test]
    [Order((int)RetrospectiveStage.Voting)]
    public async Task Screenshot_Voting() {
        this.EnsureRetrospectiveInStage(RetrospectiveStage.Grouping);

        // Given
        await this.Client1.InvokeContinueWorkflow();
        this.EnsureRetrospectiveInStage(RetrospectiveStage.Voting);

        // When
        using (IServiceScope scope = this.App.CreateTestServiceScope()) {
            await scope.TestCaseBuilder(this.RetroId).
                HasExistingParticipant("Aaron").
                HasExistingParticipant("Ashley").
                HasExistingParticipant("Hong").
                HasExistingParticipant("Josh").
                HasExistingParticipant("Patrick").
                HasExistingParticipant("Roger").
                HasExistingParticipant("Sarah").
                WithVoteOnNoteGroup("Josh", this._startAutomationGroupId).
                WithVoteOnNoteGroup("Josh", this._startAutomationGroupId).
                WithVoteOnNoteGroup("Josh", this._startAutomationGroupId).

                WithVoteOnNote("Patrick", this._startNoteDefinitionOfDoneId).
                WithVoteOnNote("Roger", this._startNoteDefinitionOfDoneId).
                WithVoteOnNote("Ashley", this._startNoteDefinitionOfDoneId).

                WithVoteOnNote("Aaron", this._startClientRetroPresenceNoteId).

                WithVoteOnNoteGroup("Aaron", this._stopBacklogUnstableNoteGroupId).
                WithVoteOnNoteGroup("Roger", this._stopBacklogUnstableNoteGroupId).
                WithVoteOnNoteGroup("Hong", this._stopBacklogUnstableNoteGroupId).
                WithVoteOnNoteGroup("Roger", this._stopBacklogUnstableNoteGroupId).

                WithVoteOnNoteGroup("Hong", this._stopSprintScopeIncreasedGroupId).
                WithVoteOnNoteGroup("Sarah", this._stopSprintScopeIncreasedGroupId).
                WithVoteOnNoteGroup("Patrick", this._stopSprintScopeIncreasedGroupId).

                WithVoteOnNoteGroup("Sarah", this._stopKickOffLongGroupId).
                WithVoteOnNoteGroup("Roger", this._stopKickOffLongGroupId).

                WithVoteOnNoteGroup("Hong", this._continueDailiesNoteGroupId).
                WithVoteOnNoteGroup("Hong", this._continueDailiesNoteGroupId).
                WithVoteOnNoteGroup("Sarah", this._continueDailiesNoteGroupId).
                WithVoteOnNoteGroup("Sarah", this._continueDailiesNoteGroupId).

                WithVoteOnNote("Roger", this._continueFrameworkNoteId).
                WithVoteOnNote("Roger", this._continueFrameworkNoteId).
                WithVoteOnNote("Patrick", this._continueFrameworkNoteId).
                WithVoteOnNote("Aaron", this._continueFrameworkNoteId).

                WithVoteOnNote("Roger", this._continueDailyBuildNoteId).
                WithVoteOnNote("Hong", this._continueDailyBuildNoteId).
                WithVoteOnNote("Sarah", this._continueDailyBuildNoteId).

                Build();
        }

        // Then
        Thread.Sleep(500);

        await CreateDocScreenshot(this.Client1.BrowserPage, "voting");
    }

    [Test]
    [Order((int)RetrospectiveStage.Finished)]
    public async Task Screenshot_Finish() {
        this.EnsureRetrospectiveInStage(RetrospectiveStage.Voting);

        // Given
        await this.Client1.InvokeContinueWorkflow();

        this.EnsureRetrospectiveInStage(RetrospectiveStage.Finished);

        // Then
        await CreateDocScreenshot(this.Client1.BrowserPage, "finish-1");

        await this.Client1.ToggleViewButton.ClickAsync();

        await CreateDocScreenshot(this.Client1.BrowserPage, "finish-2");
    }

    private static async Task CreateDocScreenshot(IPage browserPage, string name) {
        // Scroll to top, set cursor / focus to 0,0
        await browserPage.Locator(".navbar-menu").ScrollIntoViewIfNeededAsync();
        await browserPage.Keyboard.PressAsync("Tab");

        // Create a path
        string docStagingDirectory = Path.Combine(Paths.TestArtifactDir, "doc-staging");
        Directory.CreateDirectory(docStagingDirectory);

        string fileName = Path.Combine(docStagingDirectory, name + ".png");

        TestContext.WriteLine($"Creating doc screenshot: {fileName}");
        await browserPage.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = fileName,
            FullPage = true,
            Type = ScreenshotType.Png
        });
    }

    private void AddLatestNoteIdToCollection(HashSet<int> idCollection) {
        int noteId = this.GetLatestNoteId();

        idCollection.Add(noteId);
    }

    private int GetLatestNoteId() {
        Thread.Sleep(250);

        string retroId = this.RetroId;

        using IServiceScope scope = this.App.CreateTestServiceScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IReturnDbContext>();
        int noteId = dbContext.Notes.Where(x => x.Retrospective.UrlId.StringId == retroId).
            OrderByDescending(x => x.Id).
            Select(x => x.Id).
            First();
        return noteId;
    }

    private void EnsureRetrospectiveInStage(RetrospectiveStage retrospectiveStage) {
        using IServiceScope scope = this.App.CreateTestServiceScope();
        IReturnDbContext dbContext = scope.ServiceProvider.GetRequiredService<IReturnDbContext>();

        Retrospective retrospective = default;
        const int maxAttempts = 4;
        for (int i = 0; i < maxAttempts; i++)
        {
            TestContext.WriteLine($"Looking up retrospective by ID: {this.RetroId}... {i}/{maxAttempts}");
            retrospective = dbContext.Retrospectives.AsNoTracking().
                FindByRetroId(this.RetroId, CancellationToken.None).
                ConfigureAwait(false).
                GetAwaiter().
                GetResult();

            if (retrospective?.CurrentStage == retrospectiveStage) break;
            Thread.Sleep(100);
        }

        Assume.That(retrospective?.CurrentStage,
            Is.EqualTo(retrospectiveStage),
            $"Retrospective {this.RetroId} is not in stage {retrospectiveStage} required for this test. Are the tests running in the correct order?");
    }
}
