// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveStageText2.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Components {
    using Application.Retrospectives.Queries.GetRetrospectiveStatus;
    using Domain.Entities;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Rendering;

#nullable disable

    /// <summary>
    /// Represents a panel which will only render in the case the retrospective is in a certain stage
    /// </summary>
    public sealed class RetrospectiveStagePanel : ComponentBase {
        [Parameter]
        public RetrospectiveStage ApplicableTo { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [CascadingParameter]
        public RetrospectiveStatus RetrospectiveStatus { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Framework infrastructure provides argument")]
        protected override void BuildRenderTree(RenderTreeBuilder builder) {
            if (this.RetrospectiveStatus?.Stage == this.ApplicableTo) {
                this.ChildContent.Invoke(builder);
            }
        }
    }
}
