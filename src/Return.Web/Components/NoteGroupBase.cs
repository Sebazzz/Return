// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
//
//  File:           : NoteGroupBase.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Components;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Application.Common.Models;
using Application.NoteGroups.Commands;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

#nullable disable

[SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "In-app callbacks")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Set by framework")]
[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We catch, log and display.")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Needed for DI")]
public class NoteGroupBase : MediatorComponent {
    [Inject]
    public ILogger<NoteGroup> Logger { get; set; }

    [Inject]
    public IChatClient ChatClient { get; set; }

    [Parameter]
    public RetrospectiveNoteGroup Data { get; set; } = new RetrospectiveNoteGroup();

    [Parameter]
    public bool CanEdit { get; set; }

    [CascadingParameter]
    public RetroIdentifier RetroId { get; set; } = new RetroIdentifier();

    [CascadingParameter]
    public NoteLane Container { get; set; }

    [Parameter]
    public bool IsExpanded { get; set; }

    protected string Classes =>
        (this.IsExpanded ? "note-group--expanded" : "note-group--collapsed");

    public string DataTitle {
        get => this.Data.Title;
        set {
            this.Data.Title = value;
            this.ShowWarning = String.IsNullOrEmpty(value);
            this.InvokeAsync(this.UpdateTitle);
        }
    }

    private int GroupId => this.Data.Id;

    protected async Task UpdateTitle() {
        try {
            this.ShowError = false;
            await this.Mediator.Send(new UpdateNoteGroupCommand(this.RetroId.StringId, this.Data.Id, this.Data.Title));
        }
        catch (Exception ex) {
            this.Logger.LogError(ex, "Unable to save note group title of #" + this.Data.Id);
            this.ShowError = true;
        }
    }

    protected async Task AutoSummarizeNoteGroupTitle()
    {
        ChatOptions chatOptions = new()
        {
            Temperature = 0.2f,
            ResponseFormat = ChatResponseFormat.Text,
        };

        List<ChatMessage> chatMessages =
        [
            new(
                ChatRole.System,
                $@"A retrospective has been performed. Summarize the sentiment following notes grouped in the '{this.Container.Lane.Name}' lane. Use a maximum of 5 words. Don't prefix anything.
Only write a 5 word summary of the notes group. Do not use punctuations. Do not mention 'Notes' - only give a short 5 word title for these notes."
            )
        ];

        foreach (RetrospectiveNote note in this.Data.Notes)
        {
            chatMessages.Add(new(ChatRole.User, $"Note: {note.Text}"));
        }

        long startTime = Stopwatch.GetTimestamp();

        Logger.LogDebug("Invoking AI with {Count} messages", chatMessages.Count);
        try
        {
            ChatCompletion response = await this.ChatClient.CompleteAsync(chatMessages, chatOptions);
            this.Data.Title = response.ToString();
            Logger.LogTrace("Total generated summary: {RawSummary}", this.Data.Title);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error invoking AI");
        }

        Logger.LogDebug("Completed AI invocation in {Elapsed}", Stopwatch.GetElapsedTime(startTime));

        await this.UpdateTitle();
    }

    protected async Task DeleteNoteGroup() {
        try {
            await this.Mediator.Send(new DeleteNoteGroupCommand(this.RetroId.StringId, this.Data.Id));
        }
        catch (Exception ex) {
            this.Logger.LogError(ex, "Unable to delete note group #" + this.Data.Id);
            this.ShowError = true;
        }
    }

    protected void HandleDragEnter() {
        // Ignore drag to self
        if (this.Data.Id == this.Container.Payload?.GroupId) return;

        // TODO: check lane ID "no-drop"
        this.DropClass = this.Container.Payload == null ? "no-drop" : "can-drop";
    }

    protected void HandleDragLeave() {
        this.DropClass = "";
    }

    protected async Task HandleDrop() {
        this.DropClass = "";

        if (this.Container.Payload == null) return;
        if (this.GroupId == this.Container.Payload?.GroupId) return;

        await this.Container.UpdateGroupAsync(this.Data.Id);
    }

    protected void ToggleExpand() => this.IsExpanded = !this.IsExpanded;

    protected bool ShowWarning { get; private set; }
    protected bool ShowError { get; private set; }
    protected string DropClass { get; private set; }
}
