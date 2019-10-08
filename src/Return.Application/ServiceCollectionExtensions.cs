// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ServiceCollectionExtensions.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application {
    using System.Reflection;
    using AutoMapper;
    using Common.Behaviours;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Notifications;
    using Notifications.RetrospectiveJoined;
    using Retrospectives.Commands.JoinRetrospective;

    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddApplication(this IServiceCollection services) {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddMediatR(opts => opts.AsScoped(), Assembly.GetExecutingAssembly());

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestPerformanceBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
            services.AddScoped<PassphraseValidatorFactory>();

            services.AddNotificationDispatcher<RetrospectiveJoinedNotificationDispatcher>();

            return services;
        }
    }
}
