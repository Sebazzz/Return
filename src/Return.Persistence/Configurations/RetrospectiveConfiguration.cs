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
                    e.Property(x => x.StringId).HasMaxLength(32).IsUnicode(false);
                    e.HasIndex(x => x.StringId).IsUnique();
                });

            builder.Property(e => e.Title).HasMaxLength(256);
            builder.Property(e => e.HashedPassphrase).HasMaxLength(64).IsFixedLength();

            builder.UsePropertyAccessMode(PropertyAccessMode.PreferField);
        }
    }
}

