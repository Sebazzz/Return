// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteGroupTypeSecurityHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Common.Security.TypeHandling {
    using System.Diagnostics.CodeAnalysis;
    using Domain.Entities;
    using Models;

    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Instantiated by reflection")]
    [SuppressMessage("Performance",
        "CA1812:Avoid uninstantiated internal classes",
        Justification = "Dynamically instantiated")]
    internal sealed class NoteVoteTypeSecurityHandler : AbstractTypeSecurityHandler<NoteVote> {
        protected override void HandleAddOrUpdate(
            Retrospective retrospective,
            NoteVote entity,
            in CurrentParticipantModel currentParticipant
        ) {
            switch (retrospective.CurrentStage) {
                case RetrospectiveStage.Voting:
                    break;
                default:
                    throw new OperationSecurityException(
                        $"Operation not allowed in retrospective stage {retrospective.CurrentStage}");
            }
        }

        protected override void HandleDelete(
            Retrospective retrospective,
            NoteVote entity,
            in CurrentParticipantModel currentParticipant
        ) {
            switch (retrospective.CurrentStage) {
                case RetrospectiveStage.Voting:
                    break;
                default:
                    throw new OperationSecurityException(
                        $"Operation not allowed in retrospective stage {retrospective.CurrentStage}");
            }
        }
    }
}
