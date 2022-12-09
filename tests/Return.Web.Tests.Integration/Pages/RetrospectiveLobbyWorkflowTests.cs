// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveLobbyTests.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Pages;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Models;
using Application.Notes.Commands.MoveNote;
using Common;
using Components;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using NUnit.Framework;
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
        await this.WaitNavigatedToLobby();

        // Then
        await this.MultiAssert(client => client.NoteLaneElements.Expected().ToHaveCountAsync(3));
        await this.MultiAssert(client => client.BrowserPage.FindElementByTestElementId("add-note-button").Expected().ToHaveCountAsync(0));
    }

    [Test]
    public async Task RetrospectiveLobby_ShowsNoteAddButtons_OnRetrospectiveAdvancingToNextWritingStage() {
        // Given
        await Task.WhenAll(
            Task.Run(() => this.Join(this.Client1, true)),
            Task.Run(() => this.Join(this.Client2, false))
        );
        await this.WaitNavigatedToLobby();

        // When
        await this.Client1.TimeInMinutesInput.FillAsync("5");
        await this.Client1.WorkflowContinueButton.ClickAsync();

        // Then
        await this.MultiAssert(client => {
            Assert.That(() => TimeSpan.ParseExact(client.TimerText.TextContentAsync().GetAwaiter().GetResult()!, @"mm\:ss", Culture.Invariant), Is.LessThanOrEqualTo(TimeSpan.FromMinutes(5)).Retry());
            return Task.CompletedTask;
        });
        await this.MultiAssert(client => client.BrowserPage.FindElementByTestElementId("add-note-button").Expected().ToHaveCountAsync(3));
    }

    [Test]
    public async Task RetrospectiveLobby_WritingStage_CanAddNote() {
        // Given
        await this.SetRetrospective(retro => retro.CurrentStage = RetrospectiveStage.Writing);

        await Task.WhenAll(
            Task.Run(() => this.Join(this.Client1, true)),
            Task.Run(() => this.Join(this.Client2, false))
        );

        await this.WaitNavigatedToLobby();

        // When
        NoteLaneComponent noteLane = this.Client2.GetLane(KnownNoteLane.Continue);
        await noteLane.AddNoteButton.ClickAsync();

        // Then
        await this.MultiAssert(async client =>
        {
            await client.GetLane(KnownNoteLane.Continue).NoteElements.Expected().ToHaveCountAsync(1);
            await client.GetLane(KnownNoteLane.Start).NoteElements.Expected().ToHaveCountAsync(0);
            await client.GetLane(KnownNoteLane.Stop).NoteElements.Expected().ToHaveCountAsync(0);
        });

        // When
        NoteComponent note = (await noteLane.Notes()).First();
        string noteText = "some content which does not really matter to me";
        await note.Input.FillAsync(noteText);

        // Then
        NoteComponent client2Note = (await this.Client1.GetLane(KnownNoteLane.Continue).Notes()).First();
        await client2Note.Content.Expected().ToBeVisibleAsync();

        string text = await client2Note.Content.TextContentAsync();
        Assert.That(() => text, Has.Length.EqualTo(noteText.Length).And.Not.EqualTo(noteText),
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

        await this.WaitNavigatedToLobby();

        await this.MultiAssert(async client => {
            await client.GetLane(KnownNoteLane.Continue).NoteElements.Expected().ToHaveCountAsync(3);
            await client.GetLane(KnownNoteLane.Start).NoteElements.Expected().ToHaveCountAsync(1);
            await client.GetLane(KnownNoteLane.Stop).NoteElements.Expected().ToHaveCountAsync(0);
        });

        // When
        NoteLaneComponent noteLane = this.Client1.GetLane(KnownNoteLane.Continue);
        NoteComponent note = (await noteLane.Notes()).First(n => n.Id == noteId);
        await note.DeleteButton.ClickAsync();

        // Then
        await this.MultiAssert(async client => {
            NoteLaneComponent clientNoteLane = client.GetLane(KnownNoteLane.Continue);

            await clientNoteLane.NoteElements.Expected().ToHaveCountAsync(2);

            List<NoteComponent> notes = await clientNoteLane.Notes();
            Assert.That(() => notes.Select(x => x.Id).ToArray(), Does.Not.Contain(noteId).Retry());
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

        await this.WaitNavigatedToLobby();

        await this.MultiAssert(async client => {
            await client.GetLane(KnownNoteLane.Continue).NoteElements.Expected().ToHaveCountAsync(3);
            await client.GetLane(KnownNoteLane.Start).NoteElements.Expected().ToHaveCountAsync(1);
            await client.GetLane(KnownNoteLane.Stop).NoteElements.Expected().ToHaveCountAsync(0);
        });

        // When
        NoteLaneComponent noteLane = this.Client1.GetLane(KnownNoteLane.Continue);
        NoteComponent note = (await noteLane.Notes()).First(n => n.Id == noteId);

        await note.Input.FocusAsync();
        IKeyboard keyboard = note.Input.Page.Keyboard;
        await keyboard.DownAsync("Control");
        await keyboard.PressAsync("Delete");
        await keyboard.UpAsync("Control");

        // Then
        await this.MultiAssert(async client => {
            NoteLaneComponent clientNoteLane = client.GetLane(KnownNoteLane.Continue);

            await clientNoteLane.NoteElements.Expected().ToHaveCountAsync(2);

            List<NoteComponent> allNotes = await clientNoteLane.Notes();
            Assert.That(() => allNotes.Select(x => x.Id).ToArray(), Does.Not.Contain(noteId));
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

        await this.WaitNavigatedToLobby();

        // When
        int laneNumber = (int)KnownNoteLane.Continue;

        IKeyboard keyboard = this.Client2.BrowserPage.Keyboard;
        await keyboard.DownAsync("Control");
        await keyboard.PressAsync(laneNumber.ToString(CultureInfo.InvariantCulture));
        await keyboard.UpAsync("Control");

        // Then
        await this.MultiAssert(async client => {
            await client.GetLane(KnownNoteLane.Continue).NoteElements.Expected().ToHaveCountAsync(1);
            await client.GetLane(KnownNoteLane.Start).NoteElements.Expected().ToHaveCountAsync(0);
            await client.GetLane(KnownNoteLane.Stop).NoteElements.Expected().ToHaveCountAsync(0);
        });

        // When
        NoteLaneComponent noteLane = this.Client2.GetLane(KnownNoteLane.Continue);
        NoteComponent note = (await noteLane.Notes()).First();
        string noteText = "some content which does not really matter to me";
        await note.Input.FillAsync(noteText);

        // Then
        NoteComponent client2Note = (await this.Client1.GetLane(KnownNoteLane.Continue).Notes()).First();
        await client2Note.Content.Expected().ToBeVisibleAsync();

        string text = await client2Note.Content.TextContentAsync();
        Assert.That(() => text, Has.Length.EqualTo(noteText.Length).And.Not.EqualTo(noteText),
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

        await this.WaitNavigatedToLobby();

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
        await this.MultiAssert(async client => {
            NoteLaneComponent noteLane = client.GetLane(KnownNoteLane.Start);
            NoteGroupComponent noteGroup = (await noteLane.NoteGroups()).First(x => x.Id == noteGroupId);

            List<NoteComponent> notes = await noteGroup.Notes();
            await noteGroup.NoteElements.Expected().ToHaveCountAsync(2);
            Assert.That(() => notes.Select(x => x.Id).ToArray(), Contains.Item(note1Id).And.Contain(note2Id));
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

        await this.WaitNavigatedToLobby();

        // When
        NoteLaneComponent noteLane = this.Client1.GetLane(KnownNoteLane.Continue);
        await noteLane.AddNoteGroupButton.ClickAsync();

        // Then
        await this.MultiAssert(async client => {
            await client.GetLane(KnownNoteLane.Continue).NoteGroupElements.Expected().ToHaveCountAsync(1);
            await client.GetLane(KnownNoteLane.Start).NoteGroupElements.Expected().ToHaveCountAsync(1);
            await client.GetLane(KnownNoteLane.Stop).NoteGroupElements.Expected().ToHaveCountAsync(0);
        });

        // When
        int lastAddedNoteGroupId = this.App.GetLastAddedId<NoteGroup>();
        NoteGroupComponent note = (await noteLane.NoteGroups()).First(x => x.Id == lastAddedNoteGroupId);
        string noteText = "title of new notegroup";
        await note.Input.FillAsync(noteText);
        await this.Client1.Unfocus(); // Unfocus to propagate change

        // Then
        NoteGroupComponent client2NoteGroup = (await this.Client2.GetLane(KnownNoteLane.Continue).NoteGroups()).First(x => x.Id == lastAddedNoteGroupId);
        await client2NoteGroup.Title.Expected().ToHaveTextAsync(noteText);
    }
}
