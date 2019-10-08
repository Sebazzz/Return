// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetParticipantQueryHandlerTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Retrospectives.Queries {
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Retrospectives.Queries.GetParticipant;
    using Domain.Entities;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class GetParticipantQueryHandlerTests : QueryTestBase {
        private string _retro1Id;
        private string _retro2Id;

        [OneTimeSetUp]
        public async Task OneTimeSetUp() {
            var retro = new Retrospective {
                Title = "What",
                Participants =
                {
                    new Participant {Name = "John", Color = Color.BlueViolet},
                    new Participant {Name = "Jane", Color = Color.Aqua},
                },
                HashedPassphrase = "abef"
            };
            this._retro1Id = retro.UrlId.StringId;
            this.Context.Retrospectives.Add(retro);

            var retro2 = new Retrospective {
                Title = "Who",
                Participants =
                {
                    new Participant {Name = "Foo", Color = Color.BlueViolet},
                    new Participant {Name = "Baz", Color = Color.Aqua},
                },
                HashedPassphrase = "abef"
            };
            this._retro2Id = retro2.UrlId.StringId;
            this.Context.Retrospectives.Add(retro2);
            await this.Context.SaveChangesAsync(CancellationToken.None);
        }

        [Test]
        public async Task GetParticipantQueryHandler_ReturnsParticipantInfo_IfFound() {
            // Given
            var handler = new GetParticipantQueryHandler(this.Context, this.Mapper);
            var query = new GetParticipantQuery("John", this._retro1Id);

            // When
            var result = await handler.Handle(query, CancellationToken.None);

            // Then
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("John"));
        }

        [Test]
        public async Task GetParticipantQueryHandler_ReturnsNull_IfNotFound1() {
            // Given
            var handler = new GetParticipantQueryHandler(this.Context, this.Mapper);
            var query = new GetParticipantQuery("Jane", this._retro2Id);

            // When
            var result = await handler.Handle(query, CancellationToken.None);

            // Then
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetParticipantQueryHandler_ReturnsNull_IfNotFound2() {
            // Given
            var handler = new GetParticipantQueryHandler(this.Context, this.Mapper);
            var query = new GetParticipantQuery("Baz", this._retro1Id);

            // When
            var result = await handler.Handle(query, CancellationToken.None);

            // Then
            Assert.That(result, Is.Null);
        }
    }
}
