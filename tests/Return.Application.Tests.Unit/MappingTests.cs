// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : MappingTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit {
    using System.Drawing;
    using Application.Retrospective.Queries.GetParticipantsInfo;
    using Common.Models;
    using Domain.Entities;
    using NUnit.Framework;
    using PredefinedParticipantColor.Queries.GetAvailablePredefinedParticipantColors;
    using Support;

    [TestFixture]
    public sealed class MappingTests : MappingTestBase {
        [Test]
        public void ShouldHaveValidConfiguration() {
            this.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void ShouldMap_PredefinedParticipantColor_ToAvailableParticipantColor() {
            // Given
            var entity = new PredefinedParticipantColor("Color A", Color.Tomato);

            // When
            var mapped = this.Mapper.Map<AvailableParticipantColorModel>(entity);

            // Then
            Assert.That(mapped, Is.Not.Null);

            Assert.That(mapped.Name, Is.EqualTo(entity.Name));
            Assert.That(mapped.B, Is.EqualTo(entity.Color.B));
        }

        [Test]
        public void ShouldMap_Participant_ToPredefinedParticipantInfo() {
            // Given
            var entity = new Participant {
                Name = "Josh",
                Color = Color.BlueViolet
            };

            // When
            var mapped = this.Mapper.Map<ParticipantInfo>(entity);

            // Then
            Assert.That(mapped, Is.Not.Null);

            Assert.That(mapped.Name, Is.EqualTo(entity.Name));
            Assert.That(mapped.Color.R, Is.EqualTo(entity.Color.R));
            Assert.That(mapped.Color.B, Is.EqualTo(entity.Color.B));
            Assert.That(mapped.Color.G, Is.EqualTo(entity.Color.G));
        }

        [Test]
        public void ShouldMap_NoteLane_ToRetrospectiveLane() {
            // Given
            var entity = new NoteLane {
                Id = KnownNoteLane.Stop,
                Name = "Stop!!!"
            };

            // When
            var mapped = this.Mapper.Map<Application.Retrospective.Queries.GetRetrospectiveStatus.RetrospectiveLane>(entity);

            // Then
            Assert.That(mapped, Is.Not.Null);

            Assert.That(mapped.Name, Is.EqualTo(entity.Name));
            Assert.That(mapped.Id, Is.EqualTo((int)entity.Id));
        }

        [Test]
        public void ShouldMap_Note_ToRetrospectiveNote() {
            // Given
            var entity = new Note {
                Id = 1337,
                Participant = new Participant {
                    Color = Color.White,
                    Name = "Jane"
                },
                Text = "Bla bla bla"
            };

            // When
            var mapped = this.Mapper.Map<RetrospectiveNote>(entity);

            // Then
            Assert.That(mapped, Is.Not.Null);

            Assert.That(mapped.Id, Is.EqualTo((int)entity.Id));
            Assert.That(mapped.Text, Is.EqualTo(entity.Text));
            Assert.That(mapped.ParticipantName, Is.EqualTo(entity.Participant.Name));
            Assert.That(mapped.ParticipantColor.HexString, Is.EqualTo("FFFFFF"));
        }
    }
}
