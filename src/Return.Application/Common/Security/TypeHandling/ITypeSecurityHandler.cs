// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ITypeSecurityHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Common.Security.TypeHandling {
    using Domain.Entities;

    internal interface ITypeSecurityHandler {
        void HandleOperation(SecurityOperation operation, Retrospective retrospective, object entity);
    }
}
