namespace Return.Application.PredefinedParticipantColors.Queries.GetAvailablePredefinedParticipantColors {
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

    public sealed class GetAvailablePredefinedParticipantColorsRequestHandler : IRequestHandler<GetAvailablePredefinedParticipantColorsRequest, IList<AvailableParticipantColorModel>> {
        private readonly IReturnDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetAvailablePredefinedParticipantColorsRequestHandler(IReturnDbContext dbContext, IMapper mapper) {
            this._dbContext = dbContext;
            this._mapper = mapper;
        }

        public async Task<IList<AvailableParticipantColorModel>> Handle(GetAvailablePredefinedParticipantColorsRequest request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            Retrospective? retrospective = await this._dbContext.Retrospectives.Include(x => x.Participants).FindByRetroId(request.RetrospectiveId, cancellationToken).ConfigureAwait(false);
            if (retrospective == null) {
                throw new NotFoundException(nameof(Retrospective), request.RetrospectiveId);
            }

            // This looks weird, but is necessary to work around "System.ArgumentException : must be reducible node" EF bug
            var q =
                from predefinedColor in this._dbContext.PredefinedParticipantColors.AsNoTracking().AsEnumerable()
                let innerColor = predefinedColor.Color
                where !(
                    from p in retrospective.Participants
                    let pColor = p.Color
                    where pColor.R == innerColor.R && pColor.G == innerColor.G && pColor.B == innerColor.B
                    select pColor).Any()
                select predefinedColor;

            return q.AsQueryable().ProjectTo<AvailableParticipantColorModel>(this._mapper.ConfigurationProvider).ToList();
        }
    }
}
