﻿// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetParticipantsInfoCommandTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Retrospective.Queries {
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Retrospective.Queries.GetParticipantsInfo;
    using Domain.Entities;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class GetParticipantsInfoCommandTests : QueryTestBase {
        [Test]
        public async Task GetParticipantsInfoCommand_ReturnsEmptyList_OnRetrospectiveNotFound() {
            // Given
            const string retroId = "surely-not-found";
            var query = new GetParticipantsInfoCommand(retroId);
            var handler = new GetParticipantsInfoCommandHandler(this.Context, this.Mapper);

            // When
            var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(false);

            // Then
            Assert.That(result.Participants, Is.Empty);
        }

        [Test]
        public async Task GetParticipantsInfoCommand_ReturnsList_OnRetrospectiveFound() {
            // Given
            var retro = new Retrospective {
                Title = "What",
                Participants =
                {
                    new Participant {Name = "John", Color = Color.BlueViolet},
                    new Participant {Name = "Jane", Color = Color.Aqua},
                },
                HashedPassphrase = "abef"
            };
            string retroId = retro.UrlId.StringId;
            this.Context.Retrospectives.Add(retro);
            await this.Context.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

            var query = new GetParticipantsInfoCommand(retroId);
            var handler = new GetParticipantsInfoCommandHandler(this.Context, this.Mapper);

            // When
            var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(false);

            // Then
            Assert.That(result.Participants, Is.Not.Empty);
            Assert.That(result.Participants.Select(x => x.Name), Contains.Item("John"));
            Assert.That(result.Participants.Select(x => x.Name), Contains.Item("Jane"));
        }
    }
}
