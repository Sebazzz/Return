// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : UpdateNoteCommandHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notes.Commands.UpdateNote {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Common.Abstractions;
    using Common.Security;
    using Domain.Entities;
    using Domain.Services;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Notifications.NoteUpdated;

    public sealed class UpdateNoteCommandHandler : IRequestHandler<UpdateNoteCommand> {
        private readonly IReturnDbContextFactory _returnDbContext;
        private readonly ISecurityValidator _securityValidator;
        private readonly IMediator _mediator;
        private readonly ITextAnonymizingService _textAnonymizingService;

        public UpdateNoteCommandHandler(IReturnDbContextFactory returnDbContext, IMediator mediator, ISecurityValidator securityValidator, ITextAnonymizingService textAnonymizingService) {
            this._returnDbContext = returnDbContext;
            this._mediator = mediator;
            this._securityValidator = securityValidator;
            this._textAnonymizingService = textAnonymizingService;
        }

        public async Task<Unit> Handle(UpdateNoteCommand request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            using IReturnDbContext editDbContext = this._returnDbContext.CreateForEditContext();

            Note? note = await editDbContext.Notes
                .Include(x => x.Retrospective)
                .Include(x => x.Participant)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (note == null) {
                throw new NotFoundException(nameof(Note), request.Id);
            }

            // Validate operation
            await this._securityValidator.EnsureAddOrUpdate(note.Retrospective, note);

            // Update the note
            note.Text = request.Text ?? String.Empty;

            await editDbContext.SaveChangesAsync(cancellationToken);

            var notification = new NoteUpdatedNotification(NoteUpdate.FromNote(note));
            if (note.Retrospective.CurrentStage == RetrospectiveStage.Writing) {
                notification = new NoteUpdatedNotification(new NoteUpdate(note.Id, this._textAnonymizingService.AnonymizeText(notification.Note.Text)));
            }

            await this._mediator.Publish(notification, cancellationToken);

            return Unit.Value;
        }

    }
}
