// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveStageTitle.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Components {
    using Application.Retrospectives.Queries.GetRetrospectiveStatus;
    using Domain.Entities;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Rendering;

#nullable disable

    public sealed class RetrospectiveStageText : ComponentBase {
        [Parameter]
        public RetrospectiveStage ApplicableTo { get; set; }

        [Parameter]
        public string Text { get; set; }

        [CascadingParameter]
        public RetrospectiveStatus RetrospectiveStatus { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Framework infrastructure provides argument")]
        protected override void BuildRenderTree(RenderTreeBuilder builder) {
            if (this.RetrospectiveStatus?.Stage == this.ApplicableTo) {
                builder.AddContent(0, this.Text);
            }
        }
    }
}
