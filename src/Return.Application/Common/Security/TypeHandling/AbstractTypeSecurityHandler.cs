// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : AbstractTypeSecurityHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Common.Security.TypeHandling {
    using System;
    using Domain.Entities;

    internal abstract class AbstractTypeSecurityHandler<TEntity> : ITypeSecurityHandler where TEntity : class {
        public void HandleOperation(SecurityOperation operation, Retrospective retrospective, object entityObject) {
            var entity = entityObject as TEntity;

            if (entity == null) {
                return;
            }

            switch (operation) {
                case SecurityOperation.AddOrUpdate:
                    this.HandleAddOrUpdate(retrospective, entity);
                    break;
                default:
                    throw new NotImplementedException(operation.ToString());
            }
        }

        protected abstract void HandleAddOrUpdate(Retrospective retrospective, TEntity entity);
    }
}
