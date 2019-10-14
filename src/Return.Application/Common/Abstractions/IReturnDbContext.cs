// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : IReturnDbContext.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Common.Abstractions {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;

    public interface IReturnDbContext : IReturnDbContextFactory, IDisposable {
        DbSet<PredefinedParticipantColor> PredefinedParticipantColors { get; set; }
        DbSet<Note> Notes { get; set; }
        DbSet<NoteGroup> NoteGroups { get; set; }
        DbSet<NoteVote> NoteVotes { get; set; }
        DbSet<NoteLane> NoteLanes { get; set; }
        DbSet<Retrospective> Retrospectives { get; set; }
        DbSet<Participant> Participants { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
