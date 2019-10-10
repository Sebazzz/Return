// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteLaneBase.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Components {
#nullable disable

    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Application.Common.Models;
    using Application.NoteGroups.Commands;
    using Application.Notes.Commands.AddNote;
    using Application.Notifications;
    using Application.Notifications.NoteAdded;
    using Application.Notifications.NoteLaneUpdated;
    using Application.RetrospectiveLanes.Queries;
    using Application.Retrospectives.Queries.GetRetrospectiveStatus;
    using Domain.ValueObjects;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;
    using Services;

    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "In-app callbacks")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Set by framework")]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We catch, log and display.")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Needed for DI")]
    public abstract class NoteLaneBase : MediatorComponent, IDisposable, ISubscriber, INoteAddedSubscriber, INoteLaneUpdatedSubscriber {
        [Inject]
        public INotificationSubscription<INoteAddedSubscriber> NoteAddedSubscription { get; set; }

        [Inject]
        public INotificationSubscription<INoteLaneUpdatedSubscriber> NoteLaneUpdatedSubscription { get; set; }

        [Inject]
        public ILogger<NoteLane> Logger { get; set; }

        public Guid UniqueId { get; } = Guid.NewGuid();

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this.NoteAddedSubscription?.Unsubscribe(this);
                this.NoteLaneUpdatedSubscription.Unsubscribe(this);
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

        [CascadingParameter]
        public CurrentParticipantModel CurrentParticipant { get; set; }

        [CascadingParameter]
        public RetrospectiveStatus RetrospectiveStatus { get; set; }

        [CascadingParameter]
        public RetroIdentifier RetroId { get; set; }

        protected RetrospectiveLaneContent Contents { get; private set; }
        protected bool ShowErrorMessage { get; private set; }

        private bool? _isShowingNotes;

        protected override Task OnInitializedAsync() => this.Load();

        private async Task Load() => this.Contents = await this.Mediator.Send(new GetRetrospectiveLaneContentQuery(this.RetroId.StringId, this.Lane?.Id ?? 0));

        protected override void OnInitialized() {
            this.NoteLaneUpdatedSubscription.Subscribe(this);
            this.NoteAddedSubscription.Subscribe(this);
            base.OnInitialized();
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

                this.Contents.Notes.Add(result);
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
                this.Contents.Notes.Add(notification.Note);

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

        private AutoResettingBoolean _skipFirstUpdate = new AutoResettingBoolean(false);
        protected bool IsGroupingAllowed() => this.RetrospectiveStatus?.IsGroupingAllowed(this.CurrentParticipant.IsManager) == true;
        protected bool DisplayGroupHeaders() => this.Contents?.Groups.Count > 0 || this.IsGroupingAllowed();
    }
}
