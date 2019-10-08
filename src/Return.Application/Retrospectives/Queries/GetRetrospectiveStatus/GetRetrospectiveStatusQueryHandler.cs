// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetRetrospectiveStatusQueryHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Queries.GetRetrospectiveStatus {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Common;
    using Common.Abstractions;
    using Domain.Entities;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Services;

    public sealed class GetRetrospectiveStatusQueryHandler : IRequestHandler<GetRetrospectiveStatusQuery, RetrospectiveStatus> {
        private readonly IReturnDbContext _returnDbContext;
        private readonly IMapper _mapper;

        public GetRetrospectiveStatusQueryHandler(IReturnDbContext returnDbContext, IMapper mapper) {
            this._returnDbContext = returnDbContext;
            this._mapper = mapper;
        }

        public async Task<RetrospectiveStatus> Handle(GetRetrospectiveStatusQuery request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            Retrospective retrospective = await this._returnDbContext.Retrospectives.FindByRetroId(request.RetroId, cancellationToken);

            if (retrospective == null) {
                throw new NotFoundException();
            }

            var retrospectiveStatus = new RetrospectiveStatus(retrospective.UrlId.StringId, retrospective.CurrentStage, retrospective.Title);
            retrospectiveStatus.Lanes.AddRange(await this._returnDbContext.NoteLanes.AsNoTracking().ProjectTo<RetrospectiveLane>(this._mapper.ConfigurationProvider).ToListAsync(cancellationToken));

            return retrospectiveStatus;
        }
    }
}
