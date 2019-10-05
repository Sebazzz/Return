// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : Program.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.App.Commands.SeedBaseData;
    using MediatR;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Persistence;

    public static class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public static async Task Main(string[] args)
        {
            IWebHost host = CreateHostBuilder(args: args).Build();

            using (IServiceScope scope = host.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;

                try
                {
                    var returnDbContext = services.GetRequiredService<ReturnDbContext>();
                    returnDbContext.Database.Migrate();

                    var mediator = services.GetRequiredService<IMediator>();
                    await mediator.Send(new SeedBaseDataCommand(), CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
                    logger.LogError(ex, "An error occurred while migrating or initializing the database.");
                }
            }

            await host.RunAsync().ConfigureAwait(false);
        }

        private static IWebHostBuilder CreateHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args: args).UseStartup<Startup>();
    }
}
