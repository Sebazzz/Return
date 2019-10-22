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
    using Common;
    using Components;
    using Domain.Entities;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using OpenQA.Selenium;
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
    }
}
