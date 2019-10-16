// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveStatus.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Queries.GetRetrospectiveStatus {
    using System;
    using System.Collections.Generic;
    using Domain.Entities;

    public sealed class RetrospectiveStatus {
        public string RetroId { get; }

        public string Title { get; }

        public bool IsViewingOtherNotesAllowed => this.Stage >= RetrospectiveStage.Discuss;
        public bool IsViewingVotesAllowed => this.Stage >= RetrospectiveStage.Voting;
        public bool IsVotingAllowed => this.Stage == RetrospectiveStage.Voting;
        public bool IsEditingNotesAllowed => this.Stage == RetrospectiveStage.Writing;
        public bool IsDeletingNotesAllowed => this.Stage == RetrospectiveStage.Writing;
        public bool IsGroupingAllowed(bool isFacilitator) => this.Stage == RetrospectiveStage.Grouping && isFacilitator;

        public RetrospectiveWorkflowStatus WorkflowStatus { get; }

        public int VotesPerLane { get; }

        public RetrospectiveStage Stage { get; }

        public List<RetrospectiveLane> Lanes { get; } = new List<RetrospectiveLane>();

        public RetrospectiveStatus(string retroId, string title, RetrospectiveStage retrospectiveStage, RetrospectiveWorkflowStatus workflowStatus, int votesPerLane) {
            this.RetroId = retroId;
            this.Title = title;
            this.Stage = retrospectiveStage;
            this.WorkflowStatus = workflowStatus;
            this.VotesPerLane = votesPerLane;
        }

        public RetrospectiveStatus() {
            this.RetroId = String.Empty;
            this.Title = String.Empty;
            this.Stage = RetrospectiveStage.NotStarted;
            this.WorkflowStatus = new RetrospectiveWorkflowStatus();
            this.VotesPerLane = -1;
        }
    }

    public class RetrospectiveWorkflowStatus {
        public DateTimeOffset InitiationTimestamp { get; set; }

        public int TimeLimitInMinutes { get; set; }

        public bool HasReachedTimeLimit(DateTimeOffset now) => this.InitiationTimestamp.AddMinutes(this.TimeLimitInMinutes) <= now;
        public TimeSpan GetTimeLeft(DateTimeOffset now) {
            TimeSpan result = this.InitiationTimestamp.AddMinutes(this.TimeLimitInMinutes) - now;

            return result < TimeSpan.Zero ? TimeSpan.Zero : result;
        }

        internal static RetrospectiveWorkflowStatus FromDomainWorkflowData(RetrospectiveWorkflowData workflowData) =>
            new RetrospectiveWorkflowStatus {
                InitiationTimestamp = workflowData.CurrentWorkflowInitiationTimestamp,
                TimeLimitInMinutes = workflowData.CurrentWorkflowTimeLimitInMinutes
            };
    }
}
