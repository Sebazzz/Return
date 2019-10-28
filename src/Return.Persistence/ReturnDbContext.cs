// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ReturnDbContext.cs
//  Project         : Return.Persistence
// ******************************************************************************

namespace Return.Persistence {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common.Abstractions;
    using Conventions;
    using Domain.Entities;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    public sealed class ReturnDbContext : DbContext, IReturnDbContext, IEntityStateFacilitator {
        private const string SqliteProvider = "Microsoft.EntityFrameworkCore.Sqlite";

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
                switch (this._databaseOptions.DatabaseProvider) {
                    case DatabaseProvider.SqlServer:
                        optionsBuilder.UseSqlServer(this._databaseOptions.CreateConnectionString(), sql => sql.EnableRetryOnFailure());
                        break;
                    case DatabaseProvider.Sqlite:
                        SqliteConfigurator.ConfigureDbContext(optionsBuilder, this._databaseOptions);
                        break;
                    default:
                        throw new InvalidOperationException($"Invalid database provider: {this._databaseOptions.DatabaseProvider}");
                }
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
        public DbSet<NoteVote> NoteVotes { get; set; }
        public DbSet<NoteLane> NoteLanes { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Retrospective> Retrospectives { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

            // Configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReturnDbContext).Assembly);

            // Conventions
            modelBuilder.RemovePluralizingTableNameConvention();

            // Fix datetime offset support for integration tests
            // See: https://blog.dangl.me/archive/handling-datetimeoffset-in-sqlite-with-entity-framework-core/
            if (this.Database.ProviderName == SqliteProvider) {
                // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
                // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
                // To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
                // use the DateTimeOffsetToBinaryConverter
                // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
                // This only supports millisecond precision, but should be sufficient for most use cases.
                foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes()) {
                    IEnumerable<PropertyInfo> properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(DateTimeOffset));
                    foreach (PropertyInfo property in properties) {
                        if (entityType.IsOwned() == false) {
                            modelBuilder
                                .Entity(entityType.Name)
                                .Property(property.Name)
                                .HasConversion(new DateTimeOffsetToBinaryConverter());
                        }
                    }
                }
            }
        }

        public Task Reload(object entity, CancellationToken cancellationToken) {
            EntityEntry entry = this.Entry(entity);
            return entry.ReloadAsync(cancellationToken);
        }

        public IReturnDbContext CreateForEditContext() => this._databaseOptions != null ? new ReturnDbContext(this._databaseOptions) : new ReturnDbContext(this._options);

        public void Initialize() {
            if (this.Database.ProviderName == SqliteProvider) {
                this.Database.EnsureCreated();
            }
            else {
                this.Database.Migrate();
            }
        }
    }
}
