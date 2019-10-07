// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveJoinedNotification.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications {
    using MediatR;
    using Retrospectives.Queries.GetParticipantsInfo;

    public sealed class RetrospectiveJoinedNotification : INotification {
        public ParticipantInfo ParticipantInfo { get; }

        public RetrospectiveJoinedNotification(ParticipantInfo participantInfo) {
            this.ParticipantInfo = participantInfo;
        }
    }
}
