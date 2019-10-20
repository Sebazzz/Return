// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ScopeActionsExtensions.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System.Threading.Tasks;
    using Application.Retrospectives.Commands.CreateRetrospective;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    public static class ScopeActionsExtensions {
        public static async Task<string> CreateRetrospective(this IServiceScope scope, string facilitatorPassphrase) {
            scope.SetNoAuthenticationInfo();

            var command = new CreateRetrospectiveCommand {
                Title = TestContext.CurrentContext.Test.FullName,
                FacilitatorPassphrase = facilitatorPassphrase
            };

            CreateRetrospectiveCommandResponse result = await scope.Send(command);

            return result.Identifier.StringId;
        }
    }
}
