// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ServiceCollectionExtensions.cs
//  Project         : Return.Persistence
// ******************************************************************************

namespace Return.Persistence {
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Configuration;

    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration) {
            services.AddDbContext<ReturnDbContext>(optionsAction: options =>
                options.UseSqlServer(configuration.GetConnectionString("DbConnection")));

            services.AddScoped(implementationFactory: provider => provider.GetService<ReturnDbContext>());

            return services;
        }
    }
}
