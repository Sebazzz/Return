// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : UpdateNoteCommandHandlerTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Notes.Commands {
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common;
    using Application.Common.Security;
    using Application.Notes.Commands.UpdateNote;
    using Application.Notifications.NoteUpdated;
    using Domain.Entities;
    using Domain.Services;
    using MediatR;
    using NSubstitute;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class UpdateNoteCommandHandlerTests : CommandTestBase {
        [Test]
        public void UpdateNoteCommandHandler_InvalidNoteId_ThrowsNotFoundException() {
            // Given
            var command = new UpdateNoteCommand { Id = 1, Text = "HEY!" };
            var handler = new UpdateNoteCommandHandler(this.Context, Substitute.For<IMediator>(), Substitute.For<ISecurityValidator>(), Substitute.For<ITextAnonymizingService>());

            // When
            TestDelegate action = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task UpdateNoteCommandHandler_UpdatesNoteAndBroadcast_OnRequest() {
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

            var updateCommand = new UpdateNoteCommand { Id = note.Id, Text = "Updated note" };
            var mediator = Substitute.For<IMediator>();
            var securityValidator = Substitute.For<ISecurityValidator>();

            var handler = new UpdateNoteCommandHandler(this.Context, mediator, securityValidator, Substitute.For<ITextAnonymizingService>());

            // When
            await handler.Handle(updateCommand, CancellationToken.None);

            // Then
            this.Context.Entry(note).Reload();
            Assert.That(note.Text, Is.EqualTo("Updated note"));

            await securityValidator.Received().EnsureOperation(Arg.Any<Retrospective>(), SecurityOperation.AddOrUpdate, Arg.Is<Note>(n => n.Id == note.Id));
            await mediator.Received().Publish(Arg.Any<NoteUpdatedNotification>());
        }
    }
}
