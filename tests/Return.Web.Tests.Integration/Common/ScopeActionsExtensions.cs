// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ScopeActionsExtensions.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common.Abstractions;
    using Application.Retrospectives.Commands.CreateRetrospective;
    using Application.Services;
    using Domain.Entities;
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

        public static async Task SetRetrospective(this IServiceScope scope, string retroId, Action<Retrospective> action) {
            var dbContext = scope.ServiceProvider.GetRequiredService<IReturnDbContext>();

            Retrospective retrospective = await dbContext.Retrospectives.FindByRetroId(retroId, CancellationToken.None);
            action.Invoke(retrospective);
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }

        public static TestCaseBuilder TestCaseBuilder(this IServiceScope scope, string retrospectiveId) => new TestCaseBuilder(scope, retrospectiveId);
    }
}
