// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ReturnDbContext.cs
//  Project         : Return.Persistence
// ******************************************************************************

namespace Return.Persistence {
    using System;
    using Application.Common.Abstractions;
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;

    public sealed class ReturnDbContext : DbContext, IReturnDbContext {
        private readonly IDatabaseOptions? _databaseOptions;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public ReturnDbContext(DbContextOptions options) : base(options) {
        }

        public ReturnDbContext(IDatabaseOptions databaseOptions)
        {
            this._databaseOptions = databaseOptions;
        }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (this._databaseOptions != null)
            {
                optionsBuilder.UseSqlServer(this._databaseOptions.CreateConnectionString());
            }
        }

        public DbSet<NoteLane> NoteLanes { get; set; }
        public DbSet<Retrospective> Retrospectives { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReturnDbContext).Assembly);
        }
    }
}
