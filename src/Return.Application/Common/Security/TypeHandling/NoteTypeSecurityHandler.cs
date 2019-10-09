// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteTypeSecurityHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Common.Security.TypeHandling {
    using System.Diagnostics.CodeAnalysis;
    using Domain.Entities;

    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Instantiated by reflection")]
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Dynamically instantiated")]
    internal sealed class NoteTypeSecurityHandler : AbstractTypeSecurityHandler<Note> {
        protected override void HandleAddOrUpdate(Retrospective retrospective, Note entity) {
            switch (retrospective.CurrentStage) {
                case RetrospectiveStage.Writing:
                    break;
                default:
                    throw new OperationSecurityException($"Operation not allowed in retrospective stage {retrospective.CurrentStage}");
            }
        }
    }
}
