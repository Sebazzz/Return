﻿// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : UpdateNoteGroupCommandHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.NoteGroups.Commands {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Common.Abstractions;
    using Common.Security;
    using Domain.Entities;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Notifications.NoteLaneUpdated;

    public sealed class UpdateNoteGroupCommandHandler : IRequestHandler<UpdateNoteGroupCommand> {
        private readonly IReturnDbContextFactory _returnDbContextFactory;
        private readonly ISecurityValidator _securityValidator;
        private readonly IMediator _mediator;

        public UpdateNoteGroupCommandHandler(IReturnDbContextFactory returnDbContextFactory, ISecurityValidator securityValidator, IMediator mediator) {
            this._returnDbContextFactory = returnDbContextFactory;
            this._securityValidator = securityValidator;
            this._mediator = mediator;
        }

        public async Task<Unit> Handle(UpdateNoteGroupCommand request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            using IReturnDbContext dbContext = this._returnDbContextFactory.CreateForEditContext();

            // Find entity
            NoteGroup? noteGroup = await dbContext.NoteGroups
                .Include(x => x.Retrospective)
                .Include(x => x.Lane)
                .FirstOrDefaultAsync(ng => ng.Retrospective.UrlId.StringId == request.RetroId && ng.Id == request.Id, cancellationToken);
            if (noteGroup == null) {
                throw new NotFoundException(nameof(NoteGroup), request.Id);
            }

            // Validate operation
            await this._securityValidator.EnsureAddOrUpdate(noteGroup.Retrospective, noteGroup);

            // Edit
            noteGroup.Title = request.Name;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Broadcast
            await this._mediator.Publish(new NoteLaneUpdatedNotification(request.RetroId, (int)noteGroup.Lane.Id, noteGroup.Id), cancellationToken);

            return Unit.Value;
        }
    }
}
