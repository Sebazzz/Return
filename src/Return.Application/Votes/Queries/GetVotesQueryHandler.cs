// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetVotesQueryHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Votes.Queries {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Common;
    using Common.Abstractions;
    using Common.Models;
    using Domain.Entities;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Services;

    public sealed class GetVotesQueryHandler : IRequestHandler<GetVotesQuery, GetVotesQueryResult> {
        private readonly IReturnDbContext _returnDbContext;
        private readonly IMapper _mapper;

        public GetVotesQueryHandler(IReturnDbContext returnDbContext, IMapper mapper) {
            this._returnDbContext = returnDbContext;
            this._mapper = mapper;
        }

        public async Task<GetVotesQueryResult> Handle(GetVotesQuery request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            Retrospective retrospective = await this._returnDbContext.Retrospectives.AsNoTracking().FindByRetroId(request.RetroId, cancellationToken);

            if (retrospective == null) {
                throw new NotFoundException(nameof(Retrospective), request.RetroId);
            }

            List<Participant> participants = await this._returnDbContext.Participants.AsNoTracking().
                Where(x => x.Retrospective.UrlId.StringId == request.RetroId).
                ToListAsync(cancellationToken);

            List<NoteLane> lanes = await this._returnDbContext.NoteLanes.ToListAsync(cancellationToken);

            int numberOfVotesPerLane = retrospective.Options.MaximumNumberOfVotes;
            int numberOfVotes = lanes.Count * numberOfVotesPerLane;

            ILookup<int, NoteVote> votes = this._returnDbContext.NoteVotes.AsNoTracking()
                .Include(x => x.Participant)
                .Include(x => x.Note)
                .Include(x => x.Note.Lane)
                .Include(x => x.NoteGroup)
                .Include(x => x.NoteGroup.Lane)
                .Where(x => x.Retrospective.UrlId.StringId == request.RetroId)
                .AsEnumerable()
                .ToLookup(x => x.Participant.Id, x => x);

            var result = new RetrospectiveVoteStatus(numberOfVotesPerLane);

            foreach (Participant participant in participants) {
                int votesCast = 0;
                foreach (NoteVote noteVote in votes[participant.Id]) {
                    result.Votes.Add(this._mapper.Map<VoteModel>(noteVote));
                    votesCast++;
                }

                int votesLeft = numberOfVotes - votesCast;
                for (int v = 0; v < votesLeft; v++) {
                    result.Votes.Add(new VoteModel {
                        ParticipantId = participant.Id,
                        ParticipantColor = this._mapper.Map<ColorModel>(participant.Color),
                        ParticipantName = participant.Name
                    });
                }
            }

            result.Initialize();

            return new GetVotesQueryResult(result);
        }
    }
}
