// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : InitiateVotingStageCommand.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.RetrospectiveWorkflows.Commands {
    using MediatR;

    public sealed class InitiateVotingStageCommand : AbstractTimedStageCommand, IRequest {
        public int VotesPerGroup { get; set; }
    }
}
