// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteLaneUpdatedNotification.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.NoteLaneUpdated {
    using MediatR;

    public sealed class NoteLaneUpdatedNotification : INotification {
        public string RetroId { get; }
        public int LaneId { get; }
        public int GroupId { get; }

        public NoteLaneUpdatedNotification(string retroId, int laneId, int groupId) {
            this.RetroId = retroId;
            this.LaneId = laneId;
            this.GroupId = groupId;
        }
    }
}
