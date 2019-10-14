// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : MappingTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Linq;
    using Application.Common.Models;
    using Application.PredefinedParticipantColors.Queries.GetAvailablePredefinedParticipantColors;
    using Application.Retrospectives.Queries.GetParticipantsInfo;
    using Domain.Entities;
    using NUnit.Framework;
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
            var entity = new Domain.Entities.PredefinedParticipantColor("Color A", Color.Tomato);

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
            var mapped = this.Mapper.Map<Application.Retrospectives.Queries.GetRetrospectiveStatus.RetrospectiveLane>(entity);

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

        [Test]
        public void ShouldMap_NoteWithGroup_ToRetrospectiveNote() {
            // Given
            var entity = new Note {
                GroupId = 1337
            };

            // When
            var mapped = this.Mapper.Map<RetrospectiveNote>(entity);

            // Then
            Assert.That(mapped, Is.Not.Null);

            Assert.That(mapped.GroupId, Is.EqualTo(entity.GroupId));
        }

        [Test]
        public void ShouldMap_NoteGroup_ToRetrospectiveNoteGroup() {
            // Given
            var entity = new NoteGroup {
                Id = 123,
                Title = "My group"
            };

            // When
            var mapped = this.Mapper.Map<RetrospectiveNoteGroup>(entity);

            // Then
            Assert.That(mapped, Is.Not.Null);

            Assert.That(mapped.Id, Is.EqualTo((int)entity.Id));
            Assert.That(mapped.Title, Is.EqualTo(entity.Title));
        }

        [Test]
        public void ShouldMap_NoteVoteWithNote_ToVoteModel() {
            // Given
            var entity = new NoteVote
            {
                Id = 342,
                Note = new Note
                {
                    Id = 49,
                    Lane = new NoteLane
                    {
                        Id = KnownNoteLane.Stop
                    }
                },
                Participant = new Participant
                {
                    Id = 43,
                    Color = Color.Aqua,
                    Name = "Hans"
                }
            };

            // When
            var mapped = this.Mapper.Map<VoteModel>(entity);

            // Then
            Assert.That(mapped, Is.Not.Null);

            Assert.That(mapped.Id, Is.EqualTo(entity.Id));
            Assert.That(mapped.ParticipantId, Is.EqualTo(entity.Participant.Id));
            Assert.That(mapped.ParticipantName, Is.EqualTo(entity.Participant.Name));
            Assert.That(mapped.ParticipantColor.G, Is.EqualTo(entity.Participant.Color.G));

            Assert.That(mapped.LaneId, Is.EqualTo((int) KnownNoteLane.Stop));
            Assert.That(mapped.NoteId, Is.EqualTo(entity.Note.Id));
            Assert.That(mapped.NoteGroupId, Is.EqualTo(null));
            Assert.That(mapped.IsCast, Is.True);
        }

        [Test]
        public void ShouldMap_NoteVoteWithNoteGroup_ToVoteModel() {
            // Given
            var entity = new NoteVote
            {
                Id = 342,
                NoteGroup = new NoteGroup
                {
                    Id = 49,
                    Lane = new NoteLane
                    {
                        Id = KnownNoteLane.Stop
                    }
                },
                Participant = new Participant
                {
                    Id = 43,
                    Color = Color.Aqua,
                    Name = "Hans"
                }
            };

            // When
            var mapped = this.Mapper.Map<VoteModel>(entity);

            // Then
            Assert.That(mapped, Is.Not.Null);

            Assert.That(mapped.Id, Is.EqualTo(entity.Id));
            Assert.That(mapped.ParticipantId, Is.EqualTo(entity.Participant.Id));
            Assert.That(mapped.ParticipantName, Is.EqualTo(entity.Participant.Name));
            Assert.That(mapped.ParticipantColor.G, Is.EqualTo(entity.Participant.Color.G));

            Assert.That(mapped.LaneId, Is.EqualTo((int) KnownNoteLane.Stop));
            Assert.That(mapped.NoteGroupId, Is.EqualTo(entity.NoteGroup.Id));
            Assert.That(mapped.NoteId, Is.EqualTo(null));
            Assert.That(mapped.IsCast, Is.True);
        }

        [Test]
        public void ShouldMap_NoteCollection_ToGroups() {
            RetrospectiveNoteGroup Group(int id) => new RetrospectiveNoteGroup { Id = id };
            RetrospectiveNote Note(int id, int? groupId) => new RetrospectiveNote { Id = id, GroupId = groupId };

            // Given
            RetrospectiveNote[] allNotes = { Note(1, 1), Note(2, 1), Note(3, null), Note(4, 2), Note(5, null) };
            var notes = new List<RetrospectiveNote>(allNotes);
            RetrospectiveNoteGroup[] allGroups = { Group(1), Group(2), Group(3) };

            // When
            RetrospectiveNoteGroup[] mappedGroups = this.Mapper.Map<ICollection<RetrospectiveNote>, ICollection<RetrospectiveNoteGroup>>(notes, allGroups).ToArray();

            // Then
            Assert.That(mappedGroups.Select(x => x.Id), Is.EquivalentTo(new[] { 1, 2, 3 }));

            void AssertGroupContents(int groupId, params int[] noteIds) {
                var noteGroup = allGroups.FirstOrDefault(g => g.Id == groupId) ?? throw new AssertionException("Test error: Cannot find group: " + groupId);
                Assert.That(noteGroup.Notes.Select(x => x.Id), Is.EquivalentTo(noteIds),
                    $"Expected group {groupId} to contain ids {String.Join(",", noteIds)}");
            }

            AssertGroupContents(1, 1, 2);
            AssertGroupContents(2, 4);
            AssertGroupContents(3);

            Assert.That(notes.Select(x => x.Id), Is.EquivalentTo(new[] { 3, 5 }));
        }
    }
}
