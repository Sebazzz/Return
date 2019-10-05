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
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;

    public interface IReturnDbContext
    {
        DbSet<NoteLane> NoteLanes { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
