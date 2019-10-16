// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ServiceCollectionExtensions.cs
//  Project         : Return.Persistence
// ******************************************************************************

namespace Return.Persistence {
    using Application.Common.Abstractions;
    using Common;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddPersistence(this IServiceCollection services) {
            services.AddEntityFrameworkSqlServer();

            services.AddScoped(svc => new ReturnDbContext(svc.GetRequiredService<IDatabaseOptions>()));
            services.ChainInterfaceImplementation<IReturnDbContext, ReturnDbContext>();
            services.ChainInterfaceImplementation<IEntityStateFacilitator, ReturnDbContext>();
            services.ChainInterfaceImplementation<IReturnDbContextFactory, ReturnDbContext>();

            return services;
        }
    }
}
