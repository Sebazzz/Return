// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteGroupBase.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Components {
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Application.Common.Models;
    using Application.NoteGroups.Commands;
    using Domain.ValueObjects;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;

#nullable disable

    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "In-app callbacks")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Set by framework")]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We catch, log and display.")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Needed for DI")]
    public class NoteGroupBase : MediatorComponent {
        [Inject]
        public ILogger<NoteGroup> Logger { get; set; }

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
}
