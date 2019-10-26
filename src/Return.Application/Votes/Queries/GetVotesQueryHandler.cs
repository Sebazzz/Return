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
        private readonly IReturnDbContextFactory _returnDbContextFactory;
        private readonly IMapper _mapper;

        public GetVotesQueryHandler(IReturnDbContextFactory returnDbContextFactory, IMapper mapper) {
            this._returnDbContextFactory = returnDbContextFactory;
            this._mapper = mapper;
        }

        public async Task<GetVotesQueryResult> Handle(GetVotesQuery request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            using IReturnDbContext dbContext = this._returnDbContextFactory.CreateForEditContext();
            Retrospective retrospective = await dbContext.Retrospectives.AsNoTracking().FindByRetroId(request.RetroId, cancellationToken);

            if (retrospective == null) {
                throw new NotFoundException(nameof(Retrospective), request.RetroId);
            }

            List<Participant> participants = await dbContext.Participants.AsNoTracking().
                Where(x => x.Retrospective.UrlId.StringId == request.RetroId).
                ToListAsync(cancellationToken);

            List<NoteLane> lanes = await dbContext.NoteLanes.ToListAsync(cancellationToken);

            int numberOfVotesPerLane = retrospective.Options.MaximumNumberOfVotes;

            ILookup<int, NoteVote> votes = dbContext.NoteVotes.AsNoTracking()
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
                foreach (NoteLane noteLane in lanes) {
                    int votesCast = 0;
                    foreach (NoteVote noteVote in votes[participant.Id]) {
                        if ((noteVote.Note?.Lane ?? noteVote.NoteGroup?.Lane)?.Id != noteLane.Id) {
                            continue;
                        }

                        result.Votes.Add(this._mapper.Map<VoteModel>(noteVote));
                        votesCast++;
                    }

                    int votesLeft = numberOfVotesPerLane - votesCast;
                    for (int v = 0; v < votesLeft; v++) {
                        result.Votes.Add(new VoteModel {
                            ParticipantId = participant.Id,
                            ParticipantColor = this._mapper.Map<ColorModel>(participant.Color),
                            ParticipantName = participant.Name
                        });
                    }
                }
            }

            result.Initialize();

            return new GetVotesQueryResult(result);
        }
    }
}
