// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : BaseDataSeeder.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.App.Commands.SeedBaseData {
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Abstractions;
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;

    internal sealed class BaseDataSeeder {
        private readonly IReturnDbContext _returnDbContext;

        public BaseDataSeeder(IReturnDbContext returnDbContext) {
            this._returnDbContext = returnDbContext;
        }

        public async Task SeedAllAsync(CancellationToken cancellationToken) {
            if (await this._returnDbContext.NoteLanes.AnyAsync(cancellationToken).ConfigureAwait(false)) {
                return;
            }

            // Seed note lanes
            this._returnDbContext.NoteLanes.AddRange(
                new NoteLane { Id = KnownNoteLane.Start, Name = "Start" },
                new NoteLane { Id = KnownNoteLane.Stop, Name = "Stop" },
                new NoteLane { Id = KnownNoteLane.Continue, Name = "Start" }
            );

            await this._returnDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
