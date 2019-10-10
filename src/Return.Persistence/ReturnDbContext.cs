// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ReturnDbContext.cs
//  Project         : Return.Persistence
// ******************************************************************************

namespace Return.Persistence {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common.Abstractions;
    using Conventions;
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    public sealed class ReturnDbContext : DbContext, IReturnDbContext, IEntityStateManager {
        private readonly DbContextOptions _options;
        private readonly IDatabaseOptions? _databaseOptions;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public ReturnDbContext(DbContextOptions options) : base(options) {
            this._options = options;
        }

        public ReturnDbContext(IDatabaseOptions databaseOptions) {
            this._databaseOptions = databaseOptions;
        }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            // Use connection string if available
            if (this._databaseOptions != null) {
                optionsBuilder.UseSqlServer(this._databaseOptions.CreateConnectionString(), sql => sql.EnableRetryOnFailure());
            }

            // Error logging (DEBUG only)
#if DEBUG
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.EnableSensitiveDataLogging();
#endif
        }

        public DbSet<PredefinedParticipantColor> PredefinedParticipantColors { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<NoteGroup> NoteGroups { get; set; }
        public DbSet<NoteLane> NoteLanes { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Retrospective> Retrospectives { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

            // Configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReturnDbContext).Assembly);

            // Conventions
            modelBuilder.RemovePluralizingTableNameConvention();
        }

        public Task Reload(object entity, CancellationToken cancellationToken) {
            EntityEntry entry = this.Entry(entity);
            return entry.ReloadAsync(cancellationToken);
        }

        public IReturnDbContext CreateForEditContext() => this._databaseOptions != null ? new ReturnDbContext(this._databaseOptions) : new ReturnDbContext(this._options);
    }
}
