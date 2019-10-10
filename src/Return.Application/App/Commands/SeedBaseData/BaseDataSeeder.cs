// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : BaseDataSeeder.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.App.Commands.SeedBaseData {
    using System.Drawing;
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
            await this.SeedNoteLanes(cancellationToken);
            await this.SeedPredefinedParticipantColor(cancellationToken);

            await this._returnDbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task SeedNoteLanes(CancellationToken cancellationToken) {
            if (await this._returnDbContext.NoteLanes.AnyAsync(cancellationToken)) {
                return;
            }

            // Seed note lanes
            this._returnDbContext.NoteLanes.AddRange(
                new NoteLane { Id = KnownNoteLane.Start, Name = "Start" },
                new NoteLane { Id = KnownNoteLane.Stop, Name = "Stop" },
                new NoteLane { Id = KnownNoteLane.Continue, Name = "Continue" }
            );
        }

        private async Task SeedPredefinedParticipantColor(CancellationToken cancellationToken) {
            if (await this._returnDbContext.PredefinedParticipantColors.AnyAsync(cancellationToken)) {
                return;
            }

            // Seed note lanes
            this._returnDbContext.PredefinedParticipantColors.AddRange(
                new PredefinedParticipantColor("Driver red", Color.Red),
                new PredefinedParticipantColor("Analytic blue", Color.Blue),
                new PredefinedParticipantColor("Amiable green", Color.Green),
                new PredefinedParticipantColor("Expressive yellow", Color.Yellow),
                new PredefinedParticipantColor("Juicy orange", Color.DarkOrange),
                new PredefinedParticipantColor("Participator purple", Color.Purple),
                new PredefinedParticipantColor("Boring blue-gray", Color.DarkSlateGray),
                new PredefinedParticipantColor("Adapting aquatic", Color.DodgerBlue),
                new PredefinedParticipantColor("Fresh lime", Color.Lime),
                new PredefinedParticipantColor("Tomàto tomató", Color.Tomato),
                new PredefinedParticipantColor("Goldie the bird", Color.Gold),
                new PredefinedParticipantColor("Farmer wheat", Color.Wheat)
            );
        }
    }
}
