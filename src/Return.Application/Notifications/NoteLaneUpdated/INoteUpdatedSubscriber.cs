// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : INoteUpdatedSubscriber.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.NoteLaneUpdated {
    using System.Threading.Tasks;

    public interface INoteLaneUpdatedSubscriber : ISubscriber {
        Task OnNoteLaneUpdated(NoteLaneUpdatedNotification note);
    }
}
