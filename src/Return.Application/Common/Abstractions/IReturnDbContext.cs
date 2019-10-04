// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : IReturnDbContext.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Common.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IReturnDbContext
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
