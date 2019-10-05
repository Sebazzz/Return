// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : FakeDbContext.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Support {
    using System;
    using System.Threading;
    using App.Commands.SeedBaseData;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public static class ReturnDbContextFactory {
        public static ReturnDbContext Create() {
            DbContextOptions<ReturnDbContext> options = new DbContextOptionsBuilder<ReturnDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ReturnDbContext(options);

            context.Database.EnsureCreated();

            new SeedBaseDataCommandHandler(context).Handle(new SeedBaseDataCommand(), CancellationToken.None).
                ConfigureAwait(false).
                GetAwaiter().
                GetResult();

            context.SaveChanges();

            return context;
        }

        public static void Destroy(ReturnDbContext context) {
            if (context == null) throw new ArgumentNullException(nameof(context));

            context.Database.EnsureDeleted();

            context.Dispose();
        }
    }
}
