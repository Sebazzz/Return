// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveStatusMapperTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Retrospectives.Queries {
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Retrospectives.Queries.GetRetrospectiveStatus;
    using Domain.Entities;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class RetrospectiveStatusMapperTests : QueryTestBase {
        [Test]
        public void RetrospectiveStatusMapper_NullArgument_ThrowsArgumentNullException() {
            // Given
            var mapper = new RetrospectiveStatusMapper(this.Context, this.Mapper);

            // When
            TestDelegate action = () => mapper.GetRetrospectiveStatus(null, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.ArgumentNullException);
        }

        [Test]
        public async Task RetrospectiveStatusMapper_ReturnsRetrospectiveInfo() {
            // Given
            var retro = new Retrospective {
                Title = "Yet another test",
                Participants =
                {
                    new Participant { Name = "John", Color = Color.BlueViolet },
                    new Participant { Name = "Jane", Color = Color.Aqua },
                },
                HashedPassphrase = "abef",
                CurrentStage = RetrospectiveStage.Writing
            };
            string retroId = retro.UrlId.StringId;
            this.Context.Retrospectives.Add(retro);
            await this.Context.SaveChangesAsync(CancellationToken.None);

            var mapper = new RetrospectiveStatusMapper(this.Context, this.Mapper);

            // When
            var result = await mapper.GetRetrospectiveStatus(retro, CancellationToken.None);

            // Then
            Assert.That(result.Lanes, Has.Count.EqualTo(3 /* Based on seed data */));
            Assert.That(result.IsEditingNotesAllowed, Is.True);
            Assert.That(result.IsViewingOtherNotesAllowed, Is.False);
            Assert.That(result.RetroId, Is.EqualTo(retroId));
            Assert.That(result.Title, Is.EqualTo(retro.Title));
        }
    }
}
