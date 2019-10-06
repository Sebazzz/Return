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
            services.AddDbContext<ReturnDbContext>();
            services.ChainInterfaceImplementation<IReturnDbContext, ReturnDbContext>();

            return services;
        }
    }
}
