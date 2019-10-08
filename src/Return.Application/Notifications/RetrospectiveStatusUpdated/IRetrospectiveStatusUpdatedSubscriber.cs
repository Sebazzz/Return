// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : IRetrospectiveStatusUpdatedSubscriber.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.RetrospectiveStatusUpdated {
    using System.Threading.Tasks;
    using Retrospectives.Queries.GetRetrospectiveStatus;

    public interface IRetrospectiveStatusUpdatedSubscriber : ISubscriber {
        Task OnRetrospectiveStatusUpdated(RetrospectiveStatus retrospectiveStatus);
    }
}
