// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveVoteStatus.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Votes.Queries {
    public sealed class GetVotesQueryResult {
        public RetrospectiveVoteStatus VoteStatus { get; }

        public GetVotesQueryResult(RetrospectiveVoteStatus voteStatus) {
            this.VoteStatus = voteStatus;
        }
    }
}
