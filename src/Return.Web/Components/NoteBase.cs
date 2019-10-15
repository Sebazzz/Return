// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteBase.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Components {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Application.Common.Models;
    using Application.Notes.Commands.DeleteNote;
    using Application.Notes.Commands.UpdateNote;
    using Application.Notifications.NoteUpdated;
    using Application.Retrospectives.Queries.GetRetrospectiveStatus;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;
    using Microsoft.JSInterop;
    using Services;

#nullable disable

    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "In-app callbacks")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Set by framework")]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We catch, log and display.")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Needed for DI")]
    public abstract class NoteBase : SubscribingComponent<Return.Application.Notifications.NoteUpdated.INoteUpdatedSubscriber>, Return.Application.Notifications.NoteUpdated.INoteUpdatedSubscriber {
        [Inject]
        public ILogger<Note> Logger { get; set; }

        [Inject]
        public IJSRuntime JSInterop { get; set; }

        [CascadingParameter]
        public RetrospectiveStatus RetrospectiveStatus { get; set; } = new RetrospectiveStatus();

        [CascadingParameter]
        public NoteLane Container { get; set; }

        [Parameter]
        public RetrospectiveNote Data { get; set; } = new RetrospectiveNote();

        public UpdateNoteCommand Model { get; } = new UpdateNoteCommand();

        [Parameter]
        public EventCallback<RetrospectiveNote> OnDeleted { get; set; }

        [Parameter]
        public bool IsLastAddedNote { get; set; }

        protected bool ShowErrorMessage { get; private set; }

        protected ElementReference TextBoxReference { get; set; }

        private readonly AutoResettingBoolean _shouldRenderValue = new AutoResettingBoolean(true);

        protected string TextData {
            get => this.Model.Text;
            set {
                this.Model.Text = value;
                this.OnHandleNoteUpdateTyping();
            }
        }

        protected override bool ShouldRender() => this._shouldRenderValue.GetValue();

        protected override void OnParametersSet() {
            if (this.Data != null) {
                this.Model.Id = this.Data.Id;
                this.Model.Text = this.Data.Text;
            }

            base.OnParametersSet();
        }

        public Task OnNoteUpdated(NoteUpdate note) {
            if (note.Id != this.Data.Id || this.CanEdit() || this.Data == null) {
                return Task.CompletedTask;
            }

            return this.InvokeAsync(() => {
                this.Data.Text = note.Text;
                this.Model.Text = note.Text;

                this.StateHasChanged();
            });
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (this.IsLastAddedNote && this.CanEdit()) {
                await this.JSInterop.InvokeVoidAsync("retro.focusNoteElement", this.TextBoxReference);

                this.IsLastAddedNote = false;
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        protected void OnHandleNoteUpdateTyping() {
            this.Data.Text = this.Model.Text;
            this.ShowErrorMessage = false;

            // Blazor calls StateChanged (which then calls ShouldRender) after the
            // immediate executing of this method (returning the task) and after
            // the task execution. Since we don't change any other state, we try
            // to avoid much traffic and re-render by disabling rendering.
            async Task OnHandleNoteUpdateWorker() {
                try {
                    await this.Mediator.Send(this.Model);

                    this._shouldRenderValue.Set();
                }
                catch (Exception ex) {
                    // We might just have a race condition here.
                    this.ShowErrorMessage = this.CanEdit();

                    this.Logger.LogError(ex, $"Error updating note #{this.Model.Id} with text '{this.Model.Text}'");
                }
            }

            this._shouldRenderValue.Set();
            this.InvokeAsync(OnHandleNoteUpdateWorker);
        }

        protected bool CanEdit() => this.RetrospectiveStatus.IsEditingNotesAllowed && this.Data.IsOwnedByCurrentUser;
        protected bool CanView() => this.RetrospectiveStatus.IsViewingOtherNotesAllowed || this.Data.IsOwnedByCurrentUser;

        protected void HandleDragStart(RetrospectiveNote selectedNote) => this.Container.Payload = selectedNote;
        protected void HandleDragEnd() => this.Container.Payload = null;

        protected async Task Delete() {
            try {
                await this.Mediator.Send(new DeleteNoteCommand(this.RetrospectiveStatus.RetroId, this.Data.Id));
            }
            catch (Exception ex) {
                this.Logger.LogError(ex, $"Delete note #{this.Data.Id} failed");
                this.ShowErrorMessage = true;
                return;
            }

            await this.OnDeleted.InvokeAsync(this.Data);
        }
    }
}
