// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteVoteConfiguration.cs
//  Project         : Return.Persistence
// ******************************************************************************


namespace Return.Persistence.Configurations
{
    using System;
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public sealed class NoteVoteConfiguration : IEntityTypeConfiguration<NoteVote>
    {
        public void Configure(EntityTypeBuilder<NoteVote> builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.HasOne(e => e.Participant).WithMany().IsRequired().OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Note).WithMany().IsRequired().OnDelete(DeleteBehavior.Cascade);
        }
    }
}

