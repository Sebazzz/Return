// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetRetrospectiveLaneContentQueryHandlerTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.RetrospectiveLanes.Queries {
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common.Abstractions;
    using Application.RetrospectiveLanes.Queries;
    using Domain.Entities;
    using Domain.Services;
    using NSubstitute;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class GetRetrospectiveLaneContentQueryHandlerTests : QueryTestBase {
        [Test]
        public async Task GetRetrospectiveLaneContentCommand_ReturnsEmpty_RetrospectiveNotFound() {
            // Given
            const string retroId = "surely-not-found";
            var query = new GetRetrospectiveLaneContentQuery(retroId, (int)KnownNoteLane.Stop);
            var handler = new GetRetrospectiveLaneContentQueryHandler(this.Context, this.Mapper, Substitute.For<ICurrentParticipantService>(), new TextAnonymizingService());

            // When
            var result = await handler.Handle(query, CancellationToken.None);

            // Then
            Assert.That(result.Notes, Is.Empty);
        }

        [Test]
        public async Task GetRetrospectiveLaneContentCommand_ReturnsNotesAnon_RetrospectiveFound() {
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
            await this.Context.SaveChangesAsync(CancellationToken.None);

            var query = new GetRetrospectiveLaneContentQuery(retroId, (int)KnownNoteLane.Stop);
            var handler = new GetRetrospectiveLaneContentQueryHandler(this.Context, this.Mapper, Substitute.For<ICurrentParticipantService>(), new TextAnonymizingService());

            // When
            var result = await handler.Handle(query, CancellationToken.None);

            // Then
            Assert.That(result.Notes, Is.Not.Empty);
            Assert.That(result.Notes.Select(x => x.Text), Does.Not.Contain("I'm angry"));
            Assert.That(result.Notes.Select(x => x.Text), Does.Not.Contain("I'm happy"));
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
                CurrentStage = RetrospectiveStage.Discuss,
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
            await this.Context.SaveChangesAsync(CancellationToken.None);

            var ng = new NoteGroup { Title = "G1", Lane = stopLane };
            ng.Retrospective = retro;
            retro.NoteGroup.Add(ng);
            retro.Notes.Add(new Note { Lane = stopLane, Participant = participant1, Text = "OK", Group = ng, GroupId = 1 });

            await this.Context.SaveChangesAsync(CancellationToken.None);

            var query = new GetRetrospectiveLaneContentQuery(retroId, (int)KnownNoteLane.Stop);
            var handler = new GetRetrospectiveLaneContentQueryHandler(this.Context, this.Mapper, Substitute.For<ICurrentParticipantService>(), new TextAnonymizingService());

            // When
            var result = await handler.Handle(query, CancellationToken.None);

            // Then
            Assert.That(result.Notes, Is.Not.Empty);
            Assert.That(result.Notes.Select(x => x.Text), Is.EquivalentTo(new[] { "I'm angry" }));
            Assert.That(result.Notes.Select(x => x.Text), Does.Not.Contain("I'm happy"));

            Assert.That(result.Groups.Select(x => x.Title), Is.EquivalentTo(new[] { "G1" }));
            Assert.That(result.Groups.SelectMany(g => g.Notes).Select(x => x.Text), Is.EquivalentTo(new[] { "OK" }));
        }
    }
}
