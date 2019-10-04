// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ReturnDbContext.cs
//  Project         : Return.Persistence
// ******************************************************************************

namespace Return.Persistence {
    using System;
    using Microsoft.EntityFrameworkCore;

    public sealed class ReturnDbContext : DbContext {
        public ReturnDbContext(DbContextOptions options) : base(options) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReturnDbContext).Assembly);
        }
    }
}
