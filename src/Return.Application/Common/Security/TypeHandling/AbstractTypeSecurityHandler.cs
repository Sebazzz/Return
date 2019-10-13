// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : AbstractTypeSecurityHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Common.Security.TypeHandling {
    using System;
    using Domain.Entities;
    using Models;

    internal abstract class AbstractTypeSecurityHandler<TEntity> : ITypeSecurityHandler where TEntity : class {
        public void HandleOperation(SecurityOperation operation, Retrospective retrospective, object entityObject, in CurrentParticipantModel currentParticipant) {
            var entity = entityObject as TEntity;

            if (entity == null) {
                return;
            }

            switch (operation) {
                case SecurityOperation.AddOrUpdate:
                    this.HandleAddOrUpdate(retrospective, entity, currentParticipant);
                    break;
                case SecurityOperation.Delete:
                    this.HandleDelete(retrospective, entity, currentParticipant);
                    break;
                default:
                    throw new NotImplementedException(operation.ToString());
            }
        }

        protected abstract void HandleAddOrUpdate(Retrospective retrospective, TEntity entity, in CurrentParticipantModel currentParticipant);
        protected abstract void HandleDelete(Retrospective retrospective, TEntity entity, in CurrentParticipantModel currentParticipant);
    }
}
