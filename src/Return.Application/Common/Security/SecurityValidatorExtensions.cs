// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : SecurityValidatorExtensions.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Common.Security {
    using System;
    using Domain.Entities;

    public static class SecurityValidatorExtensions {
        public static void EnsureAddOrUpdate(this ISecurityValidator securityValidator, Retrospective retrospective, object entity) {
            if (securityValidator == null) throw new ArgumentNullException(nameof(securityValidator));
            securityValidator.EnsureOperation(retrospective, SecurityOperation.AddOrUpdate, entity);
        }
    }
}
