// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : MoveNoteCommandHandlerTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Notes.Commands {
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common;
    using Application.Common.Security;
    using Application.Notes.Commands.MoveNote;
    using Application.Notifications.NoteMoved;
    using Domain.Entities;
    using MediatR;
    using NSubstitute;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class MoveNoteCommandHandlerTests : CommandTestBase {
        [Test]
        public void MoveNoteCommandHandler_InvalidNoteId_ThrowsNotFoundException() {
            // Given
            var command = new MoveNoteCommand(-1, null);
            var handler = new MoveNoteCommandHandler(this.Context, Substitute.For<ISecurityValidator>(), Substitute.For<IMediator>());

            // When
            TestDelegate action = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task MoveNoteCommandHandler_InvalidGroupId_ThrowsNotFoundException() {
            // Given
            var note = new Note {
                Retrospective = new Retrospective {
                    FacilitatorHashedPassphrase = "whatever",
                    CurrentStage = RetrospectiveStage.Writing,
                    Title = this.GetType().FullName
                },
                Participant = new Participant {
                    Name = "Tester"
                },
                Lane = this.Context.NoteLanes.FirstOrDefault(),
                Text = "Derp"
            };

            TestContext.WriteLine(note.Retrospective.UrlId);
            this.Context.Notes.Add(note);
            await this.Context.SaveChangesAsync();

            var command = new MoveNoteCommand(note.Id, 0);
            var handler = new MoveNoteCommandHandler(this.Context, Substitute.For<ISecurityValidator>(), Substitute.For<IMediator>());

            // When
            TestDelegate action = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task MoveNoteCommandHandler_UpdatesNoteAndBroadcast_OnRequest() {
            // Given
            var note = new Note {
                Retrospective = new Retrospective {
                    FacilitatorHashedPassphrase = "whatever",
                    CurrentStage = RetrospectiveStage.Writing,
                    Title = this.GetType().FullName
                },
                Participant = new Participant {
                    Name = "Tester"
                },
                Lane = this.Context.NoteLanes.FirstOrDefault(),
                Text = "Derp"
            };
            var noteGroup = new NoteGroup {
                Lane = note.Lane,
                Retrospective = note.Retrospective,
                Title = "G1"
            };

            TestContext.WriteLine(note.Retrospective.UrlId);
            this.Context.Notes.Add(note);
            this.Context.NoteGroups.Add(noteGroup);
            await this.Context.SaveChangesAsync();

            var updateCommand = new MoveNoteCommand(note.Id, noteGroup.Id);
            var mediator = Substitute.For<IMediator>();
            var securityValidator = Substitute.For<ISecurityValidator>();

            var handler = new MoveNoteCommandHandler(this.Context, securityValidator, mediator);

            // When
            await handler.Handle(updateCommand, CancellationToken.None);

            // Then
            this.Context.Entry(note).Reload();
            Assert.That(note.GroupId, Is.EqualTo(noteGroup.Id));

            await securityValidator.Received().EnsureOperation(Arg.Any<Retrospective>(), SecurityOperation.AddOrUpdate, Arg.Is<NoteGroup>(n => n.Id == noteGroup.Id));
            await mediator.Received().Publish(Arg.Any<NoteMovedNotification>());
        }

        [Test]
        public async Task MoveNoteCommandHandler_RemoveGroup_UpdatesNoteAndBroadcast() {
            // Given
            var note = new Note {
                Retrospective = new Retrospective {
                    FacilitatorHashedPassphrase = "whatever",
                    CurrentStage = RetrospectiveStage.Writing,
                    Title = this.GetType().FullName
                },
                Participant = new Participant {
                    Name = "Tester"
                },
                Lane = this.Context.NoteLanes.FirstOrDefault(),
                Text = "Derp"
            };
            var noteGroup = new NoteGroup {
                Lane = note.Lane,
                Retrospective = note.Retrospective,
                Title = "G1"
            };
            note.Group = noteGroup;

            TestContext.WriteLine(note.Retrospective.UrlId);
            this.Context.Notes.Add(note);
            this.Context.NoteGroups.Add(noteGroup);
            await this.Context.SaveChangesAsync();

            var updateCommand = new MoveNoteCommand(note.Id, null);
            var mediator = Substitute.For<IMediator>();
            var securityValidator = Substitute.For<ISecurityValidator>();

            var handler = new MoveNoteCommandHandler(this.Context, securityValidator, mediator);

            // When
            await handler.Handle(updateCommand, CancellationToken.None);

            // Then
            this.Context.Entry(note).Reload();
            Assert.That(note.GroupId, Is.EqualTo(null));

            await securityValidator.Received().EnsureOperation(Arg.Any<Retrospective>(), SecurityOperation.AddOrUpdate, Arg.Is<NoteGroup>(n => n.Id == noteGroup.Id));
            await mediator.Received().Publish(Arg.Any<NoteMovedNotification>());
        }


    }
}
