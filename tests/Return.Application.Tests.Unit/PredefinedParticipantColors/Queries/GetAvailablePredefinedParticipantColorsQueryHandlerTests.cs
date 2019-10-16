// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetAvailablePredefinedParticipantColorsQueryHandlerTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.PredefinedParticipantColors.Queries {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.PredefinedParticipantColors.Queries.GetAvailablePredefinedParticipantColors;
    using Domain.Entities;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class GetAvailablePredefinedParticipantColorsQueryHandlerTests : QueryTestBase {
        [Test]
        public async Task GetAvailablePredefinedParticipantColorsTest() {
            // Given
            var retro = new Domain.Entities.Retrospective {
                CreationTimestamp = DateTimeOffset.UtcNow,
                FacilitatorHashedPassphrase = "xxx",
                Title = "xxx",
                Participants = { new Participant { Name = "John", Color = Color.Gold } }
            };
            Trace.Assert(retro.UrlId.ToString() != null);
            this.Context.Retrospectives.Add(retro);
            await this.Context.SaveChangesAsync(CancellationToken.None);

            // When
            var command = new GetAvailablePredefinedParticipantColorsQueryHandler(this.Context, this.Mapper);

            IList<AvailableParticipantColorModel> result = await command.Handle(new GetAvailablePredefinedParticipantColorsQuery(retro.UrlId.StringId), CancellationToken.None);

            // Then
            List<int> colors = result.Select(x => Color.FromArgb(255, x.R, x.G, x.B).ToArgb()).ToList();

            Assert.That(colors, Does.Not.Contains(Color.Gold.ToArgb()));
            Assert.That(colors, Does.Contain(Color.Blue.ToArgb()));
        }
    }
}
