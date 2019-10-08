// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveNotification.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications {
    using MediatR;

    public abstract class RetrospectiveNotification : INotification {
        protected RetrospectiveNotification(string retroId) {
            this.RetroId = retroId;
        }

        public string RetroId { get; }
    }
}
