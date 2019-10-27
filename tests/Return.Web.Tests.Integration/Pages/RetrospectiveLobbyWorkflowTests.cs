// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveLobbyTests.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Pages {
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Common.Models;
    using Application.Notes.Commands.MoveNote;
    using Common;
    using Components;
    using Domain.Entities;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Interactions;
    using Return.Common;

    [TestFixture]
    public sealed class RetrospectiveLobbyWorkflowTests : RetrospectiveLobbyTestsBase {
        [SetUp]
        public async Task SetUp() {
            using IServiceScope scope = this.App.CreateTestServiceScope();
            this.RetroId = await scope.CreateRetrospective("scrummaster");
        }

        [Test]
        public async Task RetrospectiveLobby_ShowsPlainBoard_OnJoiningNewRetrospective() {
            // Given
            await Task.WhenAll(
                Task.Run(() => this.Join(this.Client1, true)),
                Task.Run(() => this.Join(this.Client2, false))
            );

            // When
            this.WaitNavigatedToLobby();

            // Then
            this.MultiAssert(client => Assert.That(() => client.NoteLaneElements, Has.Count.EqualTo(3).Retry()));
            this.MultiAssert(client => Assert.That(() => client.WebDriver.FindElementsByTestElementId("add-note-button"), Has.Count.EqualTo(0).Retry()));
        }

        [Test]
        public async Task RetrospectiveLobby_ShowsNoteAddButtons_OnRetrospectiveAdvancingToNextWritingStage() {
            // Given
            await Task.WhenAll(
                Task.Run(() => this.Join(this.Client1, true)),
                Task.Run(() => this.Join(this.Client2, false))
            );
            this.WaitNavigatedToLobby();

            // When
            this.Client1.TimeInMinutesInput.SendKeys("5");
            this.Client1.WorkflowContinueButton.Click();

            // Then
            this.MultiAssert(client => Assert.That(() => TimeSpan.ParseExact(client.TimerText.Text, @"mm\:ss", Culture.Invariant), Is.LessThanOrEqualTo(TimeSpan.FromMinutes(5)).Retry()));
            this.MultiAssert(client => Assert.That(() => client.WebDriver.FindElementsByTestElementId("add-note-button"), Has.Count.EqualTo(3).Retry()));
        }

        [Test]
        public async Task RetrospectiveLobby_WritingStage_CanAddNote() {
            // Given
            await this.SetRetrospective(retro => retro.CurrentStage = RetrospectiveStage.Writing);

            await Task.WhenAll(
                Task.Run(() => this.Join(this.Client1, true)),
                Task.Run(() => this.Join(this.Client2, false))
            );

            this.WaitNavigatedToLobby();

            // When
            NoteLaneComponent noteLane = this.Client2.GetLane(KnownNoteLane.Continue);
            noteLane.AddNoteButton.Click();

            // Then
            this.MultiAssert(client => {
                Assert.That(() => client.GetLane(KnownNoteLane.Continue).NoteElements, Has.Count.EqualTo(1).Retry());
                Assert.That(() => client.GetLane(KnownNoteLane.Start).NoteElements, Has.Count.EqualTo(0).Retry());
                Assert.That(() => client.GetLane(KnownNoteLane.Stop).NoteElements, Has.Count.EqualTo(0).Retry());
            });

            // When
            NoteComponent note = noteLane.Notes.First();
            string noteText = "some content which does not really matter to me";
            note.Input.SendKeys(noteText);

            // Then
            Assert.That(() => this.Client1.GetLane(KnownNoteLane.Continue).Notes.First().Content.Text,
                Has.Length.EqualTo(noteText.Length).And.Not.EqualTo(noteText).Retry(),
                "Client 1 does not have the the garbled text from client 2");
        }

        [Test]
        public async Task RetrospectiveLobby_WritingStage_CanDeleteNote() {
            // Given
            int noteId = 0;
            using (IServiceScope scope = this.App.CreateTestServiceScope()) {
                await scope.TestCaseBuilder(this.RetroId).
                    WithParticipant("Boss", true, "scrummaster").
                    WithParticipant("Josh", false).
                    WithRetrospectiveStage(RetrospectiveStage.Writing).
                    WithNote(KnownNoteLane.Start, "Josh").
                    WithNote(KnownNoteLane.Continue, "Josh").
                    WithNote(KnownNoteLane.Continue, "Boss").
                    OutputId(id => noteId = id).
                    WithNote(KnownNoteLane.Continue, "Boss").
                    Build();
            }

            await Task.WhenAll(
                Task.Run(() => this.Join(this.Client1, true, "Boss", alreadyJoined: true)),
                Task.Run(() => this.Join(this.Client2, false, "Josh", true))
            );

            this.WaitNavigatedToLobby();

            this.MultiAssert(client => {
                Assert.That(() => client.GetLane(KnownNoteLane.Continue).NoteElements, Has.Count.EqualTo(3).Retry());
                Assert.That(() => client.GetLane(KnownNoteLane.Start).NoteElements, Has.Count.EqualTo(1).Retry());
                Assert.That(() => client.GetLane(KnownNoteLane.Stop).NoteElements, Has.Count.EqualTo(0).Retry());
            });

            // When
            NoteLaneComponent noteLane = this.Client1.GetLane(KnownNoteLane.Continue);
            NoteComponent note = noteLane.Notes.First(x => x.Id == noteId);
            note.DeleteButton.Click();

            // Then
            this.MultiAssert(client => {
                NoteLaneComponent clientNoteLane = this.Client1.GetLane(KnownNoteLane.Continue);

                Assert.That(() => clientNoteLane.NoteElements, Has.Count.EqualTo(2).Retry());
                Assert.That(() => clientNoteLane.Notes.Select(x => x.Id).ToArray(), Does.Not.Contain(noteId).Retry());
            });
        }

        [Test]
        public async Task RetrospectiveLobby_WritingStage_CanDeleteNote_Shortcut() {
            // Given
            int noteId = 0;
            using (IServiceScope scope = this.App.CreateTestServiceScope()) {
                await scope.TestCaseBuilder(this.RetroId).
                    WithParticipant("Boss", true, "scrummaster").
                    WithParticipant("Josh", false).
                    WithRetrospectiveStage(RetrospectiveStage.Writing).
                    WithNote(KnownNoteLane.Start, "Josh").
                    WithNote(KnownNoteLane.Continue, "Josh").
                    WithNote(KnownNoteLane.Continue, "Boss").
                    OutputId(id => noteId = id).
                    WithNote(KnownNoteLane.Continue, "Boss").
                    Build();
            }

            await Task.WhenAll(
                Task.Run(() => this.Join(this.Client1, true, "Boss", alreadyJoined: true)),
                Task.Run(() => this.Join(this.Client2, false, "Josh", true))
            );

            this.WaitNavigatedToLobby();

            this.MultiAssert(client => {
                Assert.That(() => client.GetLane(KnownNoteLane.Continue).NoteElements, Has.Count.EqualTo(3).Retry());
                Assert.That(() => client.GetLane(KnownNoteLane.Start).NoteElements, Has.Count.EqualTo(1).Retry());
                Assert.That(() => client.GetLane(KnownNoteLane.Stop).NoteElements, Has.Count.EqualTo(0).Retry());
            });

            // When
            NoteLaneComponent noteLane = this.Client1.GetLane(KnownNoteLane.Continue);
            NoteComponent note = noteLane.Notes.First(x => x.Id == noteId);

            new Actions(this.Client1.WebDriver)
                .KeyDown(note.Input, Keys.Control)
                .SendKeys(Keys.Delete)
                .KeyUp(Keys.Control)
                .Perform();

            // Then
            this.MultiAssert(client => {
                NoteLaneComponent clientNoteLane = client.GetLane(KnownNoteLane.Continue);

                Assert.That(() => clientNoteLane.NoteElements, Has.Count.EqualTo(2).Retry());
                Assert.That(() => clientNoteLane.Notes.Select(x => x.Id).ToArray(), Does.Not.Contain(noteId).Retry());
            });
        }

        [Test]
        public async Task RetrospectiveLobby_WritingStage_CanAddNote_Shortcut() {
            // Given
            await this.SetRetrospective(retro => retro.CurrentStage = RetrospectiveStage.Writing);

            await Task.WhenAll(
                Task.Run(() => this.Join(this.Client1, true)),
                Task.Run(() => this.Join(this.Client2, false))
            );

            this.WaitNavigatedToLobby();

            // When
            int laneNumber = (int)KnownNoteLane.Continue;
            new Actions(this.Client2.WebDriver)
                .KeyDown(Keys.Control)
                .SendKeys(laneNumber.ToString(Culture.Invariant))
                .KeyUp(Keys.Control)
                .Perform();

            // Then
            this.MultiAssert(client => {
                Assert.That(() => client.GetLane(KnownNoteLane.Continue).NoteElements, Has.Count.EqualTo(1).Retry());
                Assert.That(() => client.GetLane(KnownNoteLane.Start).NoteElements, Has.Count.EqualTo(0).Retry());
                Assert.That(() => client.GetLane(KnownNoteLane.Stop).NoteElements, Has.Count.EqualTo(0).Retry());
            });

            // When
            NoteLaneComponent noteLane = this.Client2.GetLane(KnownNoteLane.Continue);
            NoteComponent note = noteLane.Notes.First();
            string noteText = "some content which does not really matter to me";
            note.Input.SendKeys(noteText);

            // Then
            Assert.That(() => this.Client1.GetLane(KnownNoteLane.Continue).Notes.First().Content.Text,
                Has.Length.EqualTo(noteText.Length).And.Not.EqualTo(noteText).Retry(),
                "Client 1 does not have the the garbled text from client 2");
        }

        [Test]
        public async Task RetrospectiveLobby_GroupingStage_CanMoveNote() {
            // Given
            int note1Id = 0, note2Id = 0, noteGroupId = 0, bossId = 0;
            using (IServiceScope scope = this.App.CreateTestServiceScope()) {
                await scope.TestCaseBuilder(this.RetroId).
                    WithParticipant("Boss", true, "scrummaster").
                    OutputId(id => bossId = id).
                    WithParticipant("Josh", false).
                    WithParticipant("Foo", false).
                    WithParticipant("Bar", false).
                    WithParticipant("Baz", false).
                    WithRetrospectiveStage(RetrospectiveStage.Writing).
                    WithNote(KnownNoteLane.Start, "Josh").
                    OutputId(id => note1Id = id).
                    WithNote(KnownNoteLane.Continue, "Boss").
                    WithNote(KnownNoteLane.Continue, "Bar").
                    WithNote(KnownNoteLane.Continue, "Baz").
                    WithNote(KnownNoteLane.Stop, "Foo").
                    WithNote(KnownNoteLane.Start, "Foo").
                    OutputId(id => note2Id = id).
                    WithNote(KnownNoteLane.Start, "Boss").
                    WithRetrospectiveStage(RetrospectiveStage.Grouping).
                    WithNoteGroup("Boss", KnownNoteLane.Continue, "Cont name").
                    WithNoteGroup("Boss", KnownNoteLane.Start, "Some name").
                    OutputId(id => noteGroupId = id).
                    Build();
            }

            await Task.WhenAll(
                Task.Run(() => this.Join(this.Client1, true, "Boss", alreadyJoined: true)),
                Task.Run(() => this.Join(this.Client2, false, "Josh", true))
            );

            this.WaitNavigatedToLobby();

            // When
            /*{
                NoteLaneComponent noteLane = this.Client1.GetLane(KnownNoteLane.Start);
                NoteComponent note1 = noteLane.Notes.First(x => x.Id == note1Id);
                NoteComponent note2 = noteLane.Notes.First(x => x.Id == note2Id);
                NoteGroupComponent noteGroup = noteLane.NoteGroups.First(x => x.Id == noteGroupId);

                this.Client1.WebDriver.ExecuteDragAndDrop(note1.WebElement, noteGroup.WebElement);
                this.Client1.WebDriver.ExecuteDragAndDrop(note2.WebElement, noteGroup.WebElement);
            }*/ // Disable while: https://bugs.chromium.org/p/chromedriver/issues/detail?id=2695

            using (IServiceScope scope = this.App.CreateTestServiceScope()) {
                scope.SetAuthenticationInfo(new CurrentParticipantModel(bossId, "Boss", true));
                await scope.Send(new MoveNoteCommand(note1Id, noteGroupId));
                await scope.Send(new MoveNoteCommand(note2Id, noteGroupId));
            }

            // Then
            this.MultiAssert(client => {
                NoteLaneComponent noteLane = client.GetLane(KnownNoteLane.Start);
                NoteGroupComponent noteGroup = noteLane.NoteGroups.First(x => x.Id == noteGroupId);

                Assert.That(() => noteGroup.Notes.Select(x => x.Id).ToArray(), Contains.Item(note1Id).And.Contain(note2Id).Retry());
            });
        }

        [Test]
        public async Task RetrospectiveLobby_GroupingStage_CanAddNoteGroup() {
            // Given
            using (IServiceScope scope = this.App.CreateTestServiceScope()) {
                await scope.TestCaseBuilder(this.RetroId).
                    WithParticipant("Boss", true, "scrummaster").
                    WithParticipant("Josh", false).
                    WithParticipant("Foo", false).
                    WithParticipant("Bar", false).
                    WithParticipant("Baz", false).
                    WithRetrospectiveStage(RetrospectiveStage.Writing).
                    WithNote(KnownNoteLane.Start, "Josh").
                    WithNote(KnownNoteLane.Continue, "Boss").
                    WithNote(KnownNoteLane.Continue, "Bar").
                    WithNote(KnownNoteLane.Continue, "Baz").
                    WithNote(KnownNoteLane.Stop, "Foo").
                    WithNote(KnownNoteLane.Start, "Foo").
                    WithNote(KnownNoteLane.Start, "Boss").
                    WithRetrospectiveStage(RetrospectiveStage.Grouping).
                    WithNoteGroup("Boss", KnownNoteLane.Start, "Some name").
                    Build();
            }

            await Task.WhenAll(
                Task.Run(() => this.Join(this.Client1, true, "Boss", alreadyJoined: true)),
                Task.Run(() => this.Join(this.Client2, false, "Josh", true))
            );

            this.WaitNavigatedToLobby();

            // When
            NoteLaneComponent noteLane = this.Client1.GetLane(KnownNoteLane.Continue);
            noteLane.AddNoteGroupButton.Click();

            // Then
            this.MultiAssert(client => {
                Assert.That(() => client.GetLane(KnownNoteLane.Continue).NoteGroupElements, Has.Count.EqualTo(1).Retry());
                Assert.That(() => client.GetLane(KnownNoteLane.Start).NoteGroupElements, Has.Count.EqualTo(1).Retry());
                Assert.That(() => client.GetLane(KnownNoteLane.Stop).NoteGroupElements, Has.Count.EqualTo(0).Retry());
            });

            // When
            int lastAddedNoteGroupId = this.App.GetLastAddedId<NoteGroup>();
            NoteGroupComponent note = noteLane.NoteGroups.First(x => x.Id == lastAddedNoteGroupId);
            string noteText = "title of new notegroup";
            note.Input.SendKeys(noteText);
            this.Client1.Unfocus(); // Unfocus to propagate change

            // Then
            Assert.That(() => this.Client2.GetLane(KnownNoteLane.Continue).NoteGroups.First(x => x.Id == lastAddedNoteGroupId).Title.Text,
                Has.Length.EqualTo(noteText.Length).And.EqualTo(noteText).Retry(),
                "Client 1 does not have the the title text from client 2");
        }
    }
}
