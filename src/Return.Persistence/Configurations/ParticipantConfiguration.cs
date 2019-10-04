// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ParticipantConfiguration.cs
//  Project         : Return.Persistence
// ******************************************************************************

namespace Return.Persistence.Configurations {
    using System;
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public sealed class ParticipantConfiguration : IEntityTypeConfiguration<Participant> {
        public void Configure(EntityTypeBuilder<Participant> builder) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.OwnsOne(e => e.Color);

            builder.Property(e => e.Name).HasMaxLength(256).IsRequired();
        }
    }
}
