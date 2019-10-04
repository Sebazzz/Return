// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ReturnDbContext.cs
//  Project         : Return.Persistence
// ******************************************************************************

namespace Return.Persistence
{
    using Microsoft.EntityFrameworkCore;

    public sealed class ReturnDbContext : DbContext
    {
        public ReturnDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
