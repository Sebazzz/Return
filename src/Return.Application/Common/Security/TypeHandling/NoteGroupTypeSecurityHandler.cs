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
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Dynamically instantiated")]
    internal sealed class NoteGroupTypeSecurityHandler : AbstractTypeSecurityHandler<NoteGroup> {
        protected override void HandleAddOrUpdate(Retrospective retrospective, NoteGroup entity, in CurrentParticipantModel currentParticipant) {
            switch (retrospective.CurrentStage) {
                case RetrospectiveStage.Grouping:
                    if (!currentParticipant.IsFacilitator) {
                        throw new OperationSecurityException($"Operation is allowed in retrospective stage {retrospective.CurrentStage} but only by facilitator role");

                    }
                    break;
                default:
                    throw new OperationSecurityException($"Operation not allowed in retrospective stage {retrospective.CurrentStage}");
            }
        }

        protected override void HandleDelete(Retrospective retrospective, NoteGroup entity, in CurrentParticipantModel currentParticipant) {
            switch (retrospective.CurrentStage) {
                case RetrospectiveStage.Grouping:
                    if (!currentParticipant.IsFacilitator) {
                        throw new OperationSecurityException($"Operation is allowed in retrospective stage {retrospective.CurrentStage} but only by facilitator role");

                    }
                    break;
                default:
                    throw new OperationSecurityException($"Operation not allowed in retrospective stage {retrospective.CurrentStage}");
            }
        }
    }
}
