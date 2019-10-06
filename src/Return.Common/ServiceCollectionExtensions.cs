// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ServiceCollectionExtensions.cs
//  Project         : Return.Common
// ******************************************************************************

namespace Return.Common {
    using System;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions {
        public static IServiceCollection ChainInterfaceImplementation<TInterface, TImplementor>(this IServiceCollection services) where TInterface : class where TImplementor : TInterface {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services.AddScoped<TInterface>(implementationFactory: provider => provider.GetService<TImplementor>());
        }

    }
}
