// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : IEntityStateFacilitator.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Common.Abstractions {
    using System.Threading;
    using System.Threading.Tasks;

    public interface IEntityStateFacilitator {
        Task Reload(object entity, CancellationToken cancellationToken);
    }
}
