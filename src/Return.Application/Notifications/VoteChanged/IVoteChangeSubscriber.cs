// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : IVoteChangeSubscriber.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications.VoteChanged {
    using System.Threading.Tasks;

    public interface IVoteChangeSubscriber : ISubscriber {
        Task OnVoteChange(VoteChange notification);
    }
}
