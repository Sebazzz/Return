// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteGroupConfiguration.cs
//  Project         : Return.Persistence
// ******************************************************************************


namespace Return.Persistence.Configurations {
    using System;
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public sealed class NoteGroupConfiguration : IEntityTypeConfiguration<NoteGroup> {
        public void Configure(EntityTypeBuilder<NoteGroup> builder) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.HasOne(e => e.Retrospective).
                WithMany(e => e.NoteGroup).
                IsRequired().
                OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Lane).WithMany().IsRequired().OnDelete(DeleteBehavior.Restrict);

            builder.Property(e => e.Title).IsRequired().HasMaxLength(256);
        }
    }
}

