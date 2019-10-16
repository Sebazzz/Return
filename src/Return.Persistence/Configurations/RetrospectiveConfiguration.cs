// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveConfiguration.cs
//  Project         : Return.Persistence
// ******************************************************************************


namespace Return.Persistence.Configurations {
    using System;
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public sealed class RetrospectiveConfiguration : IEntityTypeConfiguration<Retrospective> {
        public void Configure(EntityTypeBuilder<Retrospective> builder) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.HasKey(e => e.Id);

            builder.OwnsOne(e => e.UrlId,
                e => {
                    e.WithOwner();
                    e.HasIndex(x => x.StringId).IsUnique();
                    e.Property(x => x.StringId).HasMaxLength(32).IsUnicode(false).IsRequired();
                });

            builder.Property(e => e.Title).HasMaxLength(256);
            builder.Property(e => e.HashedPassphrase).HasMaxLength(64).IsUnicode(false).IsRequired(false).IsFixedLength();
            builder.Property(e => e.FacilitatorHashedPassphrase).IsRequired().HasMaxLength(64).IsUnicode(false).IsFixedLength();

            builder.HasMany(e => e.Participants).
                WithOne(e => e.Retrospective).
                IsRequired().
                OnDelete(DeleteBehavior.Cascade);

            builder.OwnsOne(e => e.Options,
                e => {
                    e.WithOwner();
                });

            builder.OwnsOne(e => e.WorkflowData,
                e => {
                    e.Property(x => x.CurrentWorkflowTimeLimitInMinutes);
                    e.Property(x => x.CurrentWorkflowInitiationTimestamp);

                    e.WithOwner();
                });

            builder.UsePropertyAccessMode(PropertyAccessMode.PreferProperty);
        }
    }
}

