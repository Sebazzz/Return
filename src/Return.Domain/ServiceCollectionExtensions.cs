// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ServiceCollectionExtensions.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Domain {
    using Domain.Services;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddApplication(this IServiceCollection services) {
            services.AddTransient<IPassphraseService, PassphraseService>();

            return services;
        }
    }
}
