// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
//
//  File:           : NoteLaneBase.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Components;

#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Models;
using Application.NoteGroups.Commands;
using Application.Notes.Commands.AddNote;
using Application.Notes.Commands.MoveNote;
using Application.Notifications;
using Application.Notifications.NoteAdded;
using Application.Notifications.NoteDeleted;
using Application.Notifications.NoteLaneUpdated;
using Application.Notifications.NoteMoved;
using Application.RetrospectiveLanes.Queries;
using Application.Retrospectives.Queries.GetRetrospectiveStatus;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Services;

[SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "In-app callbacks")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Set by framework")]
[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We catch, log and display.")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Needed for DI")]
public abstract class NoteLaneBase : MediatorComponent, IDisposable, INoteAddedSubscriber, INoteLaneUpdatedSubscriber, INoteMovedSubscriber, INoteDeletedSubscriber {
    [Inject]
    public INotificationSubscription<INoteAddedSubscriber> NoteAddedSubscription { get; set; }

    [Inject]
    public INotificationSubscription<INoteLaneUpdatedSubscriber> NoteLaneUpdatedSubscription { get; set; }

    [Inject]
    public INotificationSubscription<INoteMovedSubscriber> NoteMovedSubscription { get; set; }

    [Inject]
    public INotificationSubscription<INoteDeletedSubscriber> NoteDeletedSubscription { get; set; }

    [Inject]
    public IChatClient ChatClient { get; set; }

    [Inject]
    public ILogger<NoteLane> Logger { get; set; }

    public Guid UniqueId { get; } = Guid.NewGuid();

    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            this.NoteAddedSubscription.Unsubscribe(this);
            this.NoteLaneUpdatedSubscription.Unsubscribe(this);
            this.NoteMovedSubscription.Unsubscribe(this);
            this.NoteDeletedSubscription.Unsubscribe(this);
        }

        // Release the subscription reference
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        this.NoteAddedSubscription = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    ~NoteLaneBase() {
        this.Dispose(false);
    }

    public void Dispose() {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    [Parameter]
    public RetrospectiveLane Lane { get; set; }

    [Parameter]
    public int Order { get; set; }

    [CascadingParameter]
    public CurrentParticipantModel CurrentParticipant { get; set; }

    [CascadingParameter]
    public RetrospectiveStatus RetrospectiveStatus { get; set; }

    [CascadingParameter]
    public RetroIdentifier RetroId { get; set; }

    /// <summary>
    /// Used for drag and drop
    /// </summary>
    internal RetrospectiveNote Payload { get; set; }

    protected RetrospectiveLaneContent Contents { get; private set; }

    protected RetrospectiveNote LastAddedNote { get; private set; }

    protected bool ShowErrorMessage { get; private set; }

    private bool? _isShowingNotes;

    protected override Task OnInitializedAsync() => this.Load();

    private async Task Load() => this.Contents = await this.Mediator.Send(new GetRetrospectiveLaneContentQuery(this.RetroId.StringId, this.Lane?.Id ?? 0));

    protected override void OnInitialized() {
        this.NoteLaneUpdatedSubscription.Subscribe(this);
        this.NoteAddedSubscription.Subscribe(this);
        this.NoteDeletedSubscription.Subscribe(this);
        this.NoteMovedSubscription.Subscribe(this);

        base.OnInitialized();
    }

    internal async Task UpdateGroupAsync(int? groupId) {
        if (this.Payload == null) {
            return;
        }

        int noteToMoveId = this.Payload.Id;

        if (!this.ExecuteNoteMove(noteId: noteToMoveId, newGroupId: groupId)) return;

        await this.Mediator.Send(new MoveNoteCommand(noteToMoveId, groupId));

        this.StateHasChanged();
    }

    private bool ExecuteNoteMove(int noteId, int? newGroupId) {
        // Find the source group, target group and the note
        RetrospectiveNoteGroup sourceGroup = null, targetGroup = null;
        RetrospectiveNote note = null;
        foreach (RetrospectiveNoteGroup noteGroup in this.Contents.Groups) {
            if (noteGroup.Id == newGroupId) {
                targetGroup = noteGroup;
            }

            foreach (RetrospectiveNote groupNote in noteGroup.Notes) {
                if (groupNote.Id == noteId) {
                    sourceGroup = noteGroup;
                    note = groupNote;
                }
            }

            if (sourceGroup != null && targetGroup != null) {
                break;
            }
        }

        foreach (RetrospectiveNote noGroupNote in this.Contents.Notes) {
            if (noGroupNote.Id == noteId) {
                sourceGroup = null;
                note = noGroupNote;
            }
        }

        // No need to do anything or can't do anything
        if (sourceGroup == targetGroup || note == null) {
            return false;
        }

        // Update state
        if (sourceGroup == null) {
            this.Contents.Notes.Remove(note);
            targetGroup.Notes.Add(note);
            note.GroupId = targetGroup.Id;
        }
        else if (targetGroup == null) {
            sourceGroup.Notes.Remove(note);
            this.Contents.Notes.Add(note);
            note.GroupId = null;
        }
        else {
            sourceGroup.Notes.Remove(note);
            targetGroup.Notes.Add(note);
            note.GroupId = targetGroup.Id;
        }

        return true;
    }

    public Task OnNoteMoved(NoteMovedNotification notification) {
        // Filter out irrelevant notifications
        if (notification.LaneId != this.Lane.Id ||
            notification.RetroId != this.RetroId.StringId) {
            return Task.CompletedTask;
        }

        this.InvokeAsync(() => {
            if (this.ExecuteNoteMove(notification.NoteId, notification.GroupId)) {
                this.StateHasChanged();
            }
        });

        return Task.CompletedTask;
    }

    private async Task Refresh() {
        await this.Load();
        this.StateHasChanged();
    }

    protected override void OnParametersSet() {
        base.OnParametersSet();

        if (this.RetrospectiveStatus != null) {
            this.HandleNoteChange();

            this._isShowingNotes = this.RetrospectiveStatus.IsViewingOtherNotesAllowed;
        }
    }

    /// <summary>
    /// When the notes were not revealed they actually contain garbage data. It has the same length of the words,
    /// but isn't the words. We need to reveal the notes by re-issuing a renew.
    /// </summary>
    private void HandleNoteChange() {
        if (this._isShowingNotes != null && (this.RetrospectiveStatus.IsViewingOtherNotesAllowed != this._isShowingNotes)) {
            this.InvokeAsync(this.Refresh);
        }
    }

    protected async Task AddNote() {
        try {
            this.ShowErrorMessage = false;

            RetrospectiveNote result = await this.Mediator.Send(new AddNoteCommand(this.RetroId.StringId, this.Lane.Id));

            this.Contents.Notes.Insert(0, result);
            this.LastAddedNote = result;
        }
        catch (Exception ex) {
            this.ShowErrorMessage = true;

            this.Logger.LogError(ex, $"Unable to add note for {this.RetroId} in lane {this.Lane?.Id}");
        }
    }

    protected async Task AddNoteGroup() {
        try {
            this.ShowErrorMessage = false;
            this._skipFirstUpdate.Set();

            RetrospectiveNoteGroup result = await this.Mediator.Send(new AddNoteGroupCommand(this.RetroId.StringId, this.Lane.Id));

            this.Contents.Groups.Add(result);
        }
        catch (Exception ex) {
            this.ShowErrorMessage = true;

            this.Logger.LogError(ex, $"Unable to add note group for {this.RetroId} in lane {this.Lane?.Id}");
        }
    }

    protected bool IsAutoGrouping { get; set; } = false;

    protected async Task AutoGroupNotes()
    {
        IsAutoGrouping = true;
        this.StateHasChanged();

        ChatOptions chatOptions = new()
        {
            Tools = [
                AIFunctionFactory.Create(
                    this.MakeNoteGroup,
                    new AIFunctionFactoryCreateOptions
                    {
                        Name = "Create note group",
                        Description = "Makes a group with a specified title and IDs of the relevant notes.",
                        Parameters = [
                            new("title") { Description = "The name of the group to create", IsRequired = true, ParameterType = typeof(string)},
                            new("noteIds") { Description = "An array of note IDs of the notes to put into this group", IsRequired = true, ParameterType = typeof(int[])},
                        ],
                        ReturnParameter = new()
                        {
                            Description = "Indication of the note group created",
                        }
                    }
                )
            ],
            ToolMode = ChatToolMode.RequireSpecific("Create note group"),
            TopP = 0.9f,
            Temperature = 0.2f,
            TopK = 10
        };

        List<ChatMessage> chatMessages =
        [
            new(
                ChatRole.System,
                $@"
Your task is to group notes by subject and create groups with the following rules:
- Only group notes that clearly belong together by subject.
- Ignore any note that does not fit into a group.
- Do not include the same note in more than one group.
- Assign each group a concise title (maximum of 5 words) summarizing its content.
- Use the ""Create note group"" tool to make the groups.

What now follows is a list of notes to divide into groups. Do not response with a summary, invoke the tool.
Each note is starts with [NOTE ID]. Each note ends with [END NOTE].

Example note with ID 123:
[NOTE 123] Some text here [END NOTE]"
            )
        ];

        StringBuilder stringBuilder = new();

        foreach (RetrospectiveNote note in this.Contents.Notes)
        {
            stringBuilder.AppendLine($"[NOTE {note.Id}] {note.Text} [END NOTE]");
        }

        chatMessages.Add(new ChatMessage(ChatRole.User, stringBuilder.ToString()));

        long startTime = Stopwatch.GetTimestamp();

        IChatClient client = new ChatClientBuilder()
            .UseFunctionInvocation(f =>
            {
                f.RetryOnError = true;
            })
            .UseLogging(this.Logger)
            .Use(this.ChatClient);

        Logger.LogDebug("Invoking AI with {Count} messages", chatMessages.Count);
        try
        {
            ChatCompletion response = await client.CompleteAsync(chatMessages, chatOptions);

            Logger.LogTrace("AI response: {@RawResponse}", response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error invoking AI");
        }
        finally
        {
            IsAutoGrouping = false;
        }

        Logger.LogDebug("Completed AI invocation in {Elapsed}", Stopwatch.GetElapsedTime(startTime));
    }


    private async Task<string> MakeNoteGroup(string title, int[] noteIds)
    {
        try
        {
            Logger.LogInformation("AI tool callback: Make note group with title {Title} and ids {@NoteIds}",
                title,
                noteIds);

            IEnumerable<string> notes = this.Contents.Notes.Where(x => noteIds.Contains(x.Id)).Select(x => x.Text);
            foreach (string note in notes) {
                Logger.LogDebug("Note selected for group {Title}: {NoteText}", title, note);
            }

            RetrospectiveNoteGroup result = await this.Mediator.Send(new AddNoteGroupCommand(this.RetroId.StringId, this.Lane.Id));
            result.Title = title;

            this.Contents.Groups.Add(result);
            await this.Mediator.Send(new UpdateNoteGroupCommand(this.RetroId.StringId, result.Id, title));

            foreach (int noteId in noteIds)
            {
                if (this.ExecuteNoteMove(noteId, result.Id))
                {
                    await this.Mediator.Send(new MoveNoteCommand(noteId, result.Id));
                }
            }

            return $"Created note group \"{title}\" and with notes: {String.Join(",", noteIds)}";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing AI invocation");
            return $"An error occured making group {title}";
        }
    }

    public Task OnNoteAdded(NoteAddedNotification notification) {
        if (notification.LaneId != this.Lane?.Id ||
            notification.RetroId != this.RetroId.StringId ||
            notification.Note.ParticipantId == this.CurrentParticipant.Id) {
            // We can ignore this notification if:
            // 1. This isn't our lane
            // 2. We added this notification (we caused this notification)
            // 3. This isn't our retrospective
            return Task.CompletedTask;
        }

        return this.InvokeAsync(() => {
            this.Contents.Notes.Insert(0, notification.Note);

            this.StateHasChanged();
        });
    }

    public Task OnNoteLaneUpdated(NoteLaneUpdatedNotification note) {
        if (this.RetroId?.StringId != note.RetroId ||
            this.Lane?.Id != note.LaneId ||
            this.Contents?.Groups.Exists(g => g.Id == note.GroupId) == true && this._skipFirstUpdate.GetValue()) {
            return Task.CompletedTask;
        }

        // Prevent deadlock
        this.InvokeAsync(this.Refresh);
        return Task.CompletedTask;
    }

    public Task OnNoteDeleted(NoteDeletedNotification notification) {
        if (notification.RetroId != this.RetroId.StringId ||
            notification.LaneId != this.Lane.Id) {
            return Task.CompletedTask;
        }

        this.InvokeAsync(() => {
            int num = this.Contents.Notes.RemoveAll(n => n.Id == notification.NoteId);

            if (num > 0) {
                this.StateHasChanged();
            }
        });

        return Task.CompletedTask;
    }



    protected override void OnAfterRender(bool firstRender) {
        // Reset last added note - this prevents refocus if someone else adds a new note in a different lane even
        this.LastAddedNote = null;
    }

    protected void OnNoteDeletedCallback(RetrospectiveNote note) {
        this.Contents.Notes.RemoveAll(n => n.Id == note.Id);
    }

    private readonly AutoResettingBoolean _skipFirstUpdate = new AutoResettingBoolean(false);
    protected internal bool IsGroupingAllowed() => this.RetrospectiveStatus?.IsGroupingAllowed(this.CurrentParticipant.IsFacilitator) == true;
    protected bool DisplayGroupHeaders() => this.Contents?.Groups.Count > 0 || this.IsGroupingAllowed();
}
