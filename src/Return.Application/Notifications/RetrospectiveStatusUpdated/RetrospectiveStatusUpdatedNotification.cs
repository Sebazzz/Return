// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveStatusUpdatedNotification.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.RetrospectiveStatusUpdated {
    using MediatR;
    using Retrospectives.Queries.GetRetrospectiveStatus;

    public sealed class RetrospectiveStatusUpdatedNotification : INotification {
        public RetrospectiveStatus RetrospectiveStatus { get; }

        public RetrospectiveStatusUpdatedNotification(RetrospectiveStatus retrospectiveStatus) {
            this.RetrospectiveStatus = retrospectiveStatus;
        }
    }
}
