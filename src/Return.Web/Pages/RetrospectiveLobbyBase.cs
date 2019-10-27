// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveLobbyBase.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Pages {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Application.Common;
    using Application.Common.Abstractions;
    using Application.Common.Models;
    using Application.Notifications;
    using Application.Notifications.RetrospectiveStatusUpdated;
    using Application.Notifications.VoteChanged;
    using Application.Retrospectives.Queries.GetRetrospectiveStatus;
    using Application.Votes.Queries;
    using Components;
    using Components.Layout;
    using Domain.Entities;
    using Domain.ValueObjects;
    using Microsoft.AspNetCore.Components;

    public interface IRetrospectiveLobby {
        bool ShowShowcase { get; }

        void ShowShowcaseDisplay();

        void ShowBoardDisplay();
    }

    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "In-app callbacks")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Set by framework")]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We catch, log and display.")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Needed for DI")]
    public abstract class RetrospectiveLobbyBase : MediatorComponent, IRetrospectiveStatusUpdatedSubscriber, IVoteChangeSubscriber, IRetrospectiveLobby, IDisposable {
        public Guid UniqueId { get; } = Guid.NewGuid();

#nullable disable

        [Inject]
        public INotificationSubscription<IRetrospectiveStatusUpdatedSubscriber> RetrospectiveStatusUpdatedSubscription { get; set; }

        [Inject]
        public INotificationSubscription<IVoteChangeSubscriber> VoteChangeSubscription { get; set; }

        [Inject]
        public ICurrentParticipantService CurrentParticipantService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        /// <summary>
        /// Route parameter
        /// </summary>
        [Parameter]
        public string RetroId { get; set; }

        [CascadingParameter]
        public IRetrospectiveLayout Layout { get; set; }

        public CurrentParticipantModel CurrentParticipant { get; set; }

        public RetrospectiveStatus RetrospectiveStatus { get; set; } = null;

        public RetroIdentifier RetroIdObject { get; set; }

        public RetrospectiveVoteStatus Votes { get; set; }

        protected bool HasLoaded { get; private set; }

        public bool ShowShowcase { get; private set; }

        public void ShowShowcaseDisplay() {
            this.ShowShowcase = true;

            this.StateHasChanged();
        }

        public void ShowBoardDisplay() {
            this.ShowShowcase = false;

            this.StateHasChanged();
        }

#nullable restore

        protected override void OnInitialized() {
            this.RetroIdObject = new RetroIdentifier(this.RetroId);

            this.RetrospectiveStatusUpdatedSubscription.Subscribe(this);
            this.VoteChangeSubscription.Subscribe(this);

            base.OnInitialized();
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this.RetrospectiveStatusUpdatedSubscription.Unsubscribe(this);
                this.VoteChangeSubscription.Unsubscribe(this);
            }
        }

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override async Task OnInitializedAsync() {
            CurrentParticipantModel currentParticipant = await this.CurrentParticipantService.GetParticipant();

            if (!currentParticipant.IsAuthenticated) {
                this.NavigationManager.NavigateTo("/retrospective/" + this.RetroId + "/join");
                return;
            }

            this.CurrentParticipant = currentParticipant;

            try {
                this.RetrospectiveStatus = await this.Mediator.Send(new GetRetrospectiveStatusQuery(this.RetroId));
                this.Layout?.Update(new RetrospectiveLayoutInfo(this.RetrospectiveStatus.Title, this.RetrospectiveStatus.Stage));

                if (this.RetrospectiveStatus.Stage == RetrospectiveStage.Finished) {
                    this.ShowShowcase = true;
                }

                this.Votes = (await this.Mediator.Send(new GetVotesQuery(this.RetroId))).VoteStatus;
            }
            catch (NotFoundException) {
                this.RetrospectiveStatus = null;
            }
            finally {
                this.HasLoaded = true;
            }
        }

        public Task OnRetrospectiveStatusUpdated(RetrospectiveStatus retrospectiveStatus) {
            if (this.RetrospectiveStatus?.RetroId != this.RetroId) {
                return Task.CompletedTask;
            }

            return this.InvokeAsync(async () => {
                this.RetrospectiveStatus = retrospectiveStatus;
                this.Layout?.Update(new RetrospectiveLayoutInfo(retrospectiveStatus.Title, retrospectiveStatus.Stage));

                switch (retrospectiveStatus.Stage) {
                    case RetrospectiveStage.Voting:
                        this.Votes = (await this.Mediator.Send(new GetVotesQuery(this.RetroId))).VoteStatus;
                        break;
                    case RetrospectiveStage.Finished:
                        this.ShowShowcase = true;
                        break;
                }

                this.StateHasChanged();
            });
        }

        public Task OnVoteChange(VoteChange notification) {
            if (notification.RetroId != this.RetroId) {
                return Task.CompletedTask;
            }

            this.InvokeAsync(() => {
                this.Votes.Apply(notification);

                this.StateHasChanged();
            });

            return Task.CompletedTask;
        }
    }
}
