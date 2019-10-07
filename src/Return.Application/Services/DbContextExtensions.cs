// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : DbContextExtensions.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Services {
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;

    public static class RetrospectiveQueryExtensions {
        public static Task<Retrospective> FindByRetroId(
            this IQueryable<Retrospective> queryable,
            string retroIdentifier,
            CancellationToken cancellationToken
        ) {
            if (retroIdentifier == null) throw new ArgumentNullException(nameof(retroIdentifier));

            return queryable.FirstOrDefaultAsync(predicate: x => x.UrlId.StringId == retroIdentifier,
                cancellationToken: cancellationToken);
        }
    }
}
