// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteConfiguration.cs
//  Project         : Return.Persistence
// ******************************************************************************


namespace Return.Persistence.Configurations {
    using System;
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public sealed class NoteConfiguration : IEntityTypeConfiguration<Note> {
        public void Configure(EntityTypeBuilder<Note> builder) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.HasOne(e => e.Retrospective).WithMany(e => e.Notes).IsRequired().OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.Text).IsRequired().HasMaxLength(2048);

            builder.HasOne(e => e.Lane).WithMany().IsRequired().OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Participant).WithMany().OnDelete(DeleteBehavior.Restrict);
        }
    }
}

