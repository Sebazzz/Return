// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : InitiateWritingStageCommand.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.RetrospectiveWorkflows.Commands {
    using MediatR;

    public sealed class InitiateWritingStageCommand : AbstractTimedStageCommand, IRequest {
    }
}
