// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : CustomWebApplicationFactory.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Application.Common.Abstractions;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Persistence;

    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Not necessary for tests")]
        protected override void ConfigureWebHost(IWebHostBuilder builder) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.ConfigureServices(configureServices: services => {
                // Create a new service provider.
                ServiceProvider serviceProvider =
                    new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();

                // Add a database context using an in-memory 
                // database for testing.
                services.AddDbContext<ReturnDbContext>(optionsAction: options => {
                    options.UseInMemoryDatabase(databaseName: "InMemoryDbForTesting");
                    options.UseInternalServiceProvider(serviceProvider: serviceProvider);
                });

                services.AddScoped<IReturnDbContext>(implementationFactory: provider =>
                    provider.GetService<ReturnDbContext>());

                ServiceProvider sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database
                using IServiceScope scope = sp.CreateScope();

                IServiceProvider scopedServices = scope.ServiceProvider;
                var context = scopedServices.GetRequiredService<ReturnDbContext>();
                var logger = scopedServices.
                    GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

                // Ensure the database is created.
                context.Database.EnsureCreated();

                try {
                    // Seed the database with test data.
                    //Utilities.InitializeDbForTests(context);
                }
                catch (Exception ex) {
                    logger.LogError(exception: ex,
                        "An error occurred seeding the " +
                        "database with test messages. Error: {ex.Message}");
                }
            }).UseEnvironment(environment: "Test");
        }
    }
}
