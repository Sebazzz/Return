// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : UpdateNoteGroupCommandHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.NoteGroups.Commands {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Abstractions;
    using Common.Security;
    using Domain.Entities;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Notifications.NoteLaneUpdated;

    public sealed class DeleteNoteGroupCommandHandler : IRequestHandler<DeleteNoteGroupCommand> {
        private readonly IReturnDbContextFactory _returnDbContextFactory;
        private readonly ISecurityValidator _securityValidator;
        private readonly IMediator _mediator;

        public DeleteNoteGroupCommandHandler(IReturnDbContextFactory returnDbContextFactory, ISecurityValidator securityValidator, IMediator mediator) {
            this._returnDbContextFactory = returnDbContextFactory;
            this._securityValidator = securityValidator;
            this._mediator = mediator;
        }

        public async Task<Unit> Handle(DeleteNoteGroupCommand request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            using IReturnDbContext dbContext = this._returnDbContextFactory.CreateForEditContext();

            // Find entity
            NoteGroup noteGroup = await dbContext.NoteGroups
                .Include(x => x.Retrospective)
                .Include(x => x.Lane)
                .FirstOrDefaultAsync(ng => ng.Retrospective.UrlId.StringId == request.RetroId && ng.Id == request.Id, cancellationToken);
            if (noteGroup == null) {
                // No need to stress, silently ignore this request
                return Unit.Value;
            }

            // Validate operation
            await this._securityValidator.EnsureAddOrUpdate(noteGroup.Retrospective, noteGroup);

            // Edit
            dbContext.NoteGroups.Remove(noteGroup);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Broadcast
            await this._mediator.Publish(new NoteLaneUpdatedNotification(request.RetroId, (int)noteGroup.Lane.Id, noteGroup.Id), cancellationToken);

            return Unit.Value;
        }
    }
}
