// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : AddNoteGroupCommandHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.NoteGroups.Commands {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Common;
    using Common.Abstractions;
    using Common.Models;
    using Common.Security;
    using Domain.Entities;
    using MediatR;
    using Notifications.NoteLaneUpdated;
    using Services;

    public sealed class AddNoteGroupCommandHandler : IRequestHandler<AddNoteGroupCommand, RetrospectiveNoteGroup> {
        private readonly IReturnDbContextFactory _returnDbContextFactory;
        private readonly ISecurityValidator _securityValidator;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public AddNoteGroupCommandHandler(IReturnDbContextFactory returnDbContextFactory, ISecurityValidator securityValidator, IMapper mapper, IMediator mediator) {
            this._returnDbContextFactory = returnDbContextFactory;
            this._securityValidator = securityValidator;
            this._mapper = mapper;
            this._mediator = mediator;
        }

        public async Task<RetrospectiveNoteGroup> Handle(AddNoteGroupCommand request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            using IReturnDbContext dbContext = this._returnDbContextFactory.CreateForEditContext();

            // Get the required relations
            Retrospective? retrospective = await dbContext.Retrospectives.FindByRetroId(request.RetroId, cancellationToken);
            if (retrospective == null) {
                throw new NotFoundException(nameof(Retrospective), request.RetroId);
            }

            NoteLane? noteLane = await dbContext.NoteLanes.FindAsync(new object[] { (KnownNoteLane)request.LaneId }, cancellationToken);
            if (noteLane == null) {
                throw new NotFoundException(nameof(NoteLane), request.LaneId);
            }

            // Add it after validation
            var noteGroup = new NoteGroup {
                Lane = noteLane,
                Title = String.Empty,
                Retrospective = retrospective
            };

            await this._securityValidator.EnsureAddOrUpdate(retrospective, noteGroup);

            dbContext.NoteGroups.Add(noteGroup);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map it and broadcast
            var mappedGroup = this._mapper.Map<RetrospectiveNoteGroup>(noteGroup);

            await this._mediator.Publish(new NoteLaneUpdatedNotification(request.RetroId, request.LaneId, mappedGroup.Id), cancellationToken);

            return mappedGroup;
        }
    }
}
