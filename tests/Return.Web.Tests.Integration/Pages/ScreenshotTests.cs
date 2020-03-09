// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ScreenshotTests.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Pages {
    using System;
    using System.Collections.Generic;
    using System.Drawing;
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
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Interactions;
    using OpenQA.Selenium.Support.Extensions;

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
        private readonly HashSet<int> _startAutomationGroupNoteIds = new HashSet<int>();
        private readonly HashSet<int> _stopBacklogUnstableNoteIds = new HashSet<int>();
        private readonly HashSet<int> _stopSprintScopeIncreasedNoteIds = new HashSet<int>();
        private readonly HashSet<int> _stopKickOffLongNoteIds = new HashSet<int>();
        private readonly HashSet<int> _continueDailiesNoteIds = new HashSet<int>();

        private int _startAutomationGroupId;
        private int _stopKickOffLongGroupId;
        private int _stopSprintScopeIncreasedGroupId;
        private int _stopBacklogUnstableNoteGroupId;
        private int _continueDailiesNoteGroupId;
        private int _startNoteDefinitionOfDoneId;
        private int _continueFrameworkNoteId;
        private int _startClientRetroPresenceNoteId;
        private int _continueDailyBuildNoteId;

        [SetUp]
        public void SkipOnAppVeyor() {
            if (String.Equals(Environment.GetEnvironmentVariable("APPVEYOR"), Boolean.TrueString, StringComparison.OrdinalIgnoreCase)) {
                throw new IgnoreException("AppVeyor is too slow to run this test fixture - this test is skipped on AppVeyor");
            }
        }

        [Test]
        [Order((int)RetrospectiveStage.NotStarted)]
        public void Screenshot_CreateRetrospective() {
            // Given
            using var createRetroPage = new CreateRetrospectivePage();
            createRetroPage.InitializeFrom(this.Client1);
            createRetroPage.Navigate(this.App);

            void SetResolution(IWebDriver webDriver) {
                webDriver.Manage().Window.Size = new Size(1280, 1024);
            }

            SetResolution(this.Client1.WebDriver);
            SetResolution(this.Client2.WebDriver);

            // When
            createRetroPage.RetrospectiveTitleInput.SendKeys("Sprint 1: Initial prototype");
            createRetroPage.FacilitatorPassphraseInput.SendKeys("scrummaster");
            createRetroPage.ParticipantPassphraseInput.SendKeys("secret");
            createRetroPage.WebDriver.TryCreateScreenshot();

            // Then
            CreateDocScreenshot(createRetroPage.WebDriver, "create-retro");

            createRetroPage.Submit();

            string url = createRetroPage.GetUrlShown();
            string retroId = Regex.Match(url, "/retrospective/(?<retroId>[A-z0-9]+)/join", RegexOptions.IgnoreCase).
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

            this.Join(this.Client1, true, "Roger", colorName: "Driver", submitCallback: () => CreateDocScreenshot(this.Client1.WebDriver, "join-retro"));
            this.Join(this.Client2, false, "Hong", colorName: "green");

            this.WaitNavigatedToLobby();

            this.Client1.TimeInMinutesInput.Clear();
            this.Client1.TimeInMinutesInput.SendKeys("5");
            this.Client1.InvokeContinueWorkflow();

            // When
            var writtenNoteIds = new HashSet<int>();
            void WriteNote(RetrospectiveLobby client, KnownNoteLane laneId, string text) {
                NoteLaneComponent lane = client.GetLane(laneId);

                lane.AddNoteButton.Click();

                NoteComponent addedNote = client.WebDriver.Retry(_ => {
                    NoteComponent firstNote = lane.Notes.FirstOrDefault();
                    if (firstNote?.Input != null && writtenNoteIds.Add(firstNote.Id)) {
                        return firstNote;
                    }

                    return null;
                });

                addedNote.Input.SendKeys(text);
            }

            WriteNote(this.Client2, KnownNoteLane.Continue, "Using this framework, it works very productive");

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

            WriteNote(this.Client1, KnownNoteLane.Start, "Daily builds should include automated smoke tests");
            this.AddLatestNoteIdToCollection(this._startAutomationGroupNoteIds);

            WriteNote(this.Client1, KnownNoteLane.Start, "Client should be present in retrospective");
            this._startClientRetroPresenceNoteId = this.GetLatestNoteId();

            WriteNote(this.Client2, KnownNoteLane.Continue, "Regular publish to acceptance environment");
            this._continueDailyBuildNoteId = this.GetLatestNoteId();

            CreateDocScreenshot(this.Client2.WebDriver, "writing");

            // Then
            this.Client1.InvokeContinueWorkflow();
        }

        [Test]
        [Order((int)RetrospectiveStage.Grouping)]
        public async Task Screenshot_Grouping() {
            this.EnsureRetrospectiveInStage(RetrospectiveStage.Discuss);

            // Given
            this.Client1.InvokeContinueWorkflow();

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
            this.Client1.TimeInMinutesInput.SendKeys("\b4");
            this.Client1.VoteCountInput.SendKeys("'\b3");

            CreateDocScreenshot(this.Client1.WebDriver, "grouping");
        }

        [Test]
        [Order((int)RetrospectiveStage.Voting)]
        public async Task Screenshot_Voting() {
            this.EnsureRetrospectiveInStage(RetrospectiveStage.Grouping);

            // Given
            this.Client1.InvokeContinueWorkflow();

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

            CreateDocScreenshot(this.Client1.WebDriver, "voting");
        }

        [Test]
        [Order((int)RetrospectiveStage.Finished)]
        public void Screenshot_Finish() {
            this.EnsureRetrospectiveInStage(RetrospectiveStage.Voting);

            // Given
            this.Client1.InvokeContinueWorkflow();

            this.EnsureRetrospectiveInStage(RetrospectiveStage.Finished);

            // Then
            CreateDocScreenshot(this.Client1.WebDriver, "finish-1");

            this.Client1.ToggleViewButton.Click();

            CreateDocScreenshot(this.Client1.WebDriver, "finish-2");
        }

        private static void CreateDocScreenshot(IWebDriver webDriver, string name) {
            if (webDriver == null) throw new ArgumentNullException(nameof(webDriver));

            // Scroll to top, set cursor / focus to 0,0
            Thread.Sleep(1000);
            webDriver.ExecuteJavaScript("window.scrollTo(0, 0)");
            new Actions(webDriver).MoveToElement(webDriver.FindElement(By.ClassName("navbar-menu")), 0, 0, MoveToElementOffsetOrigin.Center).Click().Perform();
            Thread.Sleep(1000);

            // Create a path
            string docStagingDirectory = Path.Combine(Paths.TestArtifactDir, "doc-staging");
            Directory.CreateDirectory(docStagingDirectory);

            string fileName = Path.Combine(docStagingDirectory, name + ".png");

            TestContext.WriteLine($"Creating doc screenshot: {fileName}");
            webDriver.TakeScreenshot().SaveAsFile(fileName, ScreenshotImageFormat.Png);
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
            var dbContext = scope.ServiceProvider.GetRequiredService<IReturnDbContext>();
            Assume.That(() => dbContext.Retrospectives.AsNoTracking().FindByRetroId(this.RetroId, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult(),
                Has.Property(nameof(Retrospective.CurrentStage)).EqualTo(retrospectiveStage).Retry(),
                $"Retrospective {this.RetroId} is not in stage {retrospectiveStage} required for this test. Are the tests running in the correct order?");
        }
    }
}
