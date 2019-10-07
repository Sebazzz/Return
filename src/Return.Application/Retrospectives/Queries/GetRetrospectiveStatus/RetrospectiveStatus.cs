// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveStatus.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Queries.GetRetrospectiveStatus {
    using System.Collections.Generic;
    using Domain.Entities;

    public sealed class RetrospectiveStatus {
        private readonly RetrospectiveStage _retrospectiveStage;

        public string RetroId { get; }

        public string Title { get; }

        public bool IsViewingOtherNotesAllowed => this._retrospectiveStage >= RetrospectiveStage.Discuss;
        public bool IsEditingNotesAllowed => this._retrospectiveStage == RetrospectiveStage.Writing;

        public List<RetrospectiveLane> Lanes { get; } = new List<RetrospectiveLane>();

        public RetrospectiveStatus(string retroId, RetrospectiveStage retrospectiveStage, string title) {
            this.RetroId = retroId;
            this._retrospectiveStage = retrospectiveStage;
            this.Title = title;
        }
    }
}
