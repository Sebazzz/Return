// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetRetrospectiveStatusQueryHandlerTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Retrospectives.Queries {
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common;
    using Application.Retrospectives.Queries.GetRetrospectiveStatus;
    using Domain.Entities;
    using NSubstitute;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class GetRetrospectiveStatusQueryHandlerTests : QueryTestBase {
        [Test]
        public void GetRetrospectiveStatusCommand_ThrowsNotFoundException_WhenNotFound() {
            // Given
            const string retroId = "surely-not-found";
            var query = new GetRetrospectiveStatusQuery(retroId);
            var handler = new GetRetrospectiveStatusQueryHandler(this.Context, Substitute.For<IRetrospectiveStatusMapper>());

            // When
            TestDelegate action = () => handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task GetRetrospectiveStatusCommand_ReturnsRetrospectiveInfo() {
            // Given
            var retro = new Retrospective {
                Title = "Yet another test",
                Participants =
                {
                    new Participant { Name = "John", Color = Color.BlueViolet },
                    new Participant { Name = "Jane", Color = Color.Aqua },
                },
                FacilitatorHashedPassphrase = "abef",
                HashedPassphrase = "abef",
                CurrentStage = RetrospectiveStage.Writing
            };
            string retroId = retro.UrlId.StringId;
            this.Context.Retrospectives.Add(retro);
            await this.Context.SaveChangesAsync(CancellationToken.None);

            var query = new GetRetrospectiveStatusQuery(retroId);
            var handler = new GetRetrospectiveStatusQueryHandler(this.Context, new RetrospectiveStatusMapper(this.Context, this.Mapper));

            // When
            var result = await handler.Handle(query, CancellationToken.None);

            // Then
            Assert.That(result.Lanes, Has.Count.EqualTo(3 /* Based on seed data */));
            Assert.That(result.IsEditingNotesAllowed, Is.True);
            Assert.That(result.IsViewingOtherNotesAllowed, Is.False);
            Assert.That(result.RetroId, Is.EqualTo(retroId));
            Assert.That(result.Title, Is.EqualTo(retro.Title));
        }
    }
}
