// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetRetrospectiveLaneContentQueryHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.RetrospectiveLanes.Queries {
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Common.Abstractions;
    using Common.Models;
    using Domain.Entities;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public sealed class GetRetrospectiveLaneContentQueryHandler : IRequestHandler<GetRetrospectiveLaneContentQuery, RetrospectiveLaneContent> {
        private readonly IReturnDbContext _returnDbContext;
        private readonly IMapper _mapper;

        public GetRetrospectiveLaneContentQueryHandler(IReturnDbContext returnDbContext, IMapper mapper) {
            this._returnDbContext = returnDbContext;
            this._mapper = mapper;
        }

        public async Task<RetrospectiveLaneContent> Handle(GetRetrospectiveLaneContentQuery request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var laneId = (KnownNoteLane)request.LaneId;

            var query =
                from note in this._returnDbContext.Notes
                where note.Retrospective.UrlId.StringId == request.RetroId
                where note.Lane.Id == laneId
                orderby note.CreationTimestamp
                select note;

            var lane = new RetrospectiveLaneContent();
            lane.Notes.AddRange(
                await query.ProjectTo<RetrospectiveNote>(this._mapper.ConfigurationProvider).ToListAsync(cancellationToken).ConfigureAwait(false)
            );

            return lane;
        }
    }
}
