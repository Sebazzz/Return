// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteVoteConfiguration.cs
//  Project         : Return.Persistence
// ******************************************************************************


namespace Return.Persistence.Configurations {
    using System;
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public sealed class NoteVoteConfiguration : IEntityTypeConfiguration<NoteVote> {
        public void Configure(EntityTypeBuilder<NoteVote> builder) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.HasOne(e => e.Participant).WithMany().IsRequired().HasForeignKey(e => e.ParticipantId).OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Note).WithMany().IsRequired(false).OnDelete(DeleteBehavior.ClientCascade);

            builder.HasOne(e => e.NoteGroup).WithMany().IsRequired(false).OnDelete(DeleteBehavior.ClientCascade);

            builder.HasOne(e => e.Retrospective).WithMany(r => r.NoteVotes).IsRequired().OnDelete(DeleteBehavior.Cascade);

        }
    }
}

