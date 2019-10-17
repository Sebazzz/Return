// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ShowcaseBase.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Components {
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Application.Showcase.Queries;
    using Domain.ValueObjects;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;

    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "In-app callbacks")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Set by framework")]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We catch, log and display.")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Needed for DI")]
    public abstract class ShowcaseBase : MediatorComponent {
#nullable disable
        [Inject]
        public ILogger<ShowcaseBase> Logger { get; set; }

        [CascadingParameter]
        public RetroIdentifier RetroIdentifier { get; set; }
#nullable restore

        protected ShowcaseData? Data { get; private set; }

        protected bool ShowErrorMessage { get; private set; }

        protected int VisibleItemIndex { get; private set; }

        protected override async Task OnInitializedAsync() {
            Debug.Assert(this.RetroIdentifier != null);

            try {
                GetShowcaseQueryResult result =
                    await this.Mediator.Send(new GetShowcaseQuery(this.RetroIdentifier.StringId));

                this.Data = result.Showcase;
            }
            catch (Exception ex) {
                this.ShowErrorMessage = true;
                this.Logger.LogError(ex, "Failed to load showcase");
            }
        }

        protected void ShowPreviousItem() {
            if (this.Data == null) {
                return;
            }

            int newItemIndex = this.VisibleItemIndex - 1;
            if (newItemIndex < 0) {
                newItemIndex = this.Data.Items.Count - 1;
            }

            this.VisibleItemIndex = newItemIndex;
        }

        protected void ShowNextItem() {
            if (this.Data == null) {
                return;
            }

            int newItemIndex = this.VisibleItemIndex + 1;
            if (newItemIndex >= this.Data.Items.Count) {
                newItemIndex = 0;
            }

            this.VisibleItemIndex = newItemIndex;
        }

        protected bool HasNextItem => (this.VisibleItemIndex + 1) < (this.Data?.Items.Count ?? 0) - 1;
        protected bool HasPreviousItem => this.VisibleItemIndex > 0;
    }
}
