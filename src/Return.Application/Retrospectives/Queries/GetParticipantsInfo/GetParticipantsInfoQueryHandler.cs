// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetParticipantsInfoQueryHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Queries.GetParticipantsInfo {
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Common.Abstractions;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public sealed class GetParticipantsInfoQueryHandler : IRequestHandler<GetParticipantsInfoQuery, ParticipantsInfoList> {
        private readonly IReturnDbContext _returnDbContext;
        private readonly IMapper _mapper;

        public GetParticipantsInfoQueryHandler(IReturnDbContext returnDbContext, IMapper mapper) {
            this._returnDbContext = returnDbContext;
            this._mapper = mapper;
        }

        public async Task<ParticipantsInfoList> Handle(GetParticipantsInfoQuery request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var returnValue = new ParticipantsInfoList();
            returnValue.Participants.AddRange(
                await this._returnDbContext.Retrospectives
                    .Where(r => r.UrlId.StringId == request.RetroId)
                    .SelectMany(r => r.Participants)
                    .ProjectTo<ParticipantInfo>(this._mapper.ConfigurationProvider)
                    .OrderBy(x => x.Name)
                    .ToListAsync(cancellationToken)
            );

            return returnValue;
        }
    }
}
