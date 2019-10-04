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
            builder.Property(e => e.Id).IsRequired().IsFixedLength().HasMaxLength(32);

            builder.UsePropertyAccessMode(PropertyAccessMode.PreferField);
        }
    }
}

