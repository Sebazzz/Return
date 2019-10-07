// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetRetrospectiveLaneContentCommandTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.RetrospectiveLane.Queries {
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using RetrospectiveLanes.Queries;
    using Common;
    using Domain.Entities;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class GetRetrospectiveLaneContentCommandTests : QueryTestBase {
        [Test]
        public async Task GetRetrospectiveLaneContentCommand_ReturnsEmpty_RetrospectiveNotFound() {
            // Given
            const string retroId = "surely-not-found";
            var query = new GetRetrospectiveLaneContentCommand(retroId, (int)KnownNoteLane.Stop);
            var handler = new GetRetrospectiveLaneContentCommandHandler(this.Context, this.Mapper);

            // When
            var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(false);

            // Then
            Assert.That(result.Notes, Is.Empty);
        }

        [Test]
        public async Task GetRetrospectiveLaneContentCommand_ReturnsNotes_RetrospectiveFound() {
            // Given
            var participant1 = new Participant { Name = "John", Color = Color.BlueViolet };
            var stopLane = this.Context.NoteLanes.Find(KnownNoteLane.Stop);
            var startLane = this.Context.NoteLanes.Find(KnownNoteLane.Start);
            var retro = new Retrospective {
                Title = "Yet another test",
                Participants =
                {
                    participant1,
                    new Participant {Name = "Jane", Color = Color.Aqua},
                },
                HashedPassphrase = "abef",
                CurrentStage = RetrospectiveStage.Writing,
                Notes =
                {
                    new Note
                    {
                        Lane = stopLane,
                        Participant = participant1,
                        Text = "I'm angry",
                    },
                    new Note
                    {
                        Lane = startLane,
                        Participant = participant1,
                        Text = "I'm happy",
                    }
                }
            };
            string retroId = retro.UrlId.StringId;
            this.Context.Retrospectives.Add(retro);
            await this.Context.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

            var query = new GetRetrospectiveLaneContentCommand(retroId, (int)KnownNoteLane.Stop);
            var handler = new GetRetrospectiveLaneContentCommandHandler(this.Context, this.Mapper);

            // When
            var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(false);

            // Then
            Assert.That(result.Notes, Is.Not.Empty);
            Assert.That(result.Notes.Select(x => x.Text), Contains.Item("I'm angry"));
            Assert.That(result.Notes.Select(x => x.Text), Does.Not.Contain("I'm happy"));
        }
    }
}
