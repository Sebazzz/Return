// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : MoveNoteCommandHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notes.Commands.MoveNote {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Common.Abstractions;
    using Common.Security;
    using Domain.Entities;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Notifications.NoteMoved;

    public sealed class MoveNoteCommandHandler : IRequestHandler<MoveNoteCommand> {
        private readonly IReturnDbContextFactory _returnDbContextFactory;
        private readonly ISecurityValidator _securityValidator;
        private readonly IMediator _mediator;

        public MoveNoteCommandHandler(IReturnDbContextFactory returnDbContextFactory, ISecurityValidator securityValidator, IMediator mediator) {
            this._returnDbContextFactory = returnDbContextFactory;
            this._securityValidator = securityValidator;
            this._mediator = mediator;
        }

        public async Task<Unit> Handle(MoveNoteCommand request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            IReturnDbContext dbContext = this._returnDbContextFactory.CreateForEditContext();

            // Get data
            Note? note = await dbContext.Notes
                .Include(x => x.Lane)
                .Include(x => x.Group)
                .Include(x => x.Retrospective)
                .FirstOrDefaultAsync(x => x.Id == request.NoteId, cancellationToken);

            if (note == null) {
                throw new NotFoundException(nameof(Note), request.NoteId);
            }

            NoteGroup? newTargetGroup = null;

            if (request.GroupId != null) {
                newTargetGroup = await dbContext.NoteGroups.Include(x => x.Lane).
                    FirstOrDefaultAsync(x => x.Id == request.GroupId, cancellationToken);

                if (newTargetGroup == null) {
                    throw new NotFoundException(nameof(NoteGroup), request.GroupId.Value);
                }
            }

            // Validate
            if (newTargetGroup != null && note.Lane.Id != newTargetGroup.Lane.Id) {
                throw new InvalidOperationException("Invalid move command: this would result in a lane change.");
            }

            NoteGroup involvedGroup = newTargetGroup ??
                                      note.Group ??
                                      throw new InvalidOperationException("Note move from ungrouped to ungrouped");
            await this._securityValidator.EnsureAddOrUpdate(note.Retrospective, involvedGroup);

            // Update
            note.GroupId = newTargetGroup?.Id;
            note.Group = newTargetGroup;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Broadcast
            var broadcast =
                new NoteMovedNotification(note.Retrospective.UrlId.StringId, (int)note.Lane.Id, note.Id, note.GroupId);
            await this._mediator.Publish(broadcast, cancellationToken);

            return Unit.Value;
        }
    }
}
