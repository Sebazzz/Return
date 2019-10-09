// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : IEntityStateManager.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Common.Abstractions {
    using System.Threading;
    using System.Threading.Tasks;

    public interface IEntityStateManager {
        Task Reload(object entity, CancellationToken cancellationToken);
    }
}
