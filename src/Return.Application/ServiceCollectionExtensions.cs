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
    using Common.Security;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Notifications;
    using Retrospectives.Queries.GetRetrospectiveStatus;
    using RetrospectiveWorkflows.Common;

    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddApplication(this IServiceCollection services) {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddMediatR(opts => opts.AsScoped(), Assembly.GetExecutingAssembly());

            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestPerformanceBehaviour<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehaviour<,>));

            services.AddScoped<IRetrospectiveStatusMapper, RetrospectiveStatusMapper>();
            services.AddScoped<IRetrospectiveStatusUpdateDispatcher, RetrospectiveStatusUpdateDispatcher>();
            services.AddScoped<ISecurityValidator, SecurityValidator>();

            services.AddNotificationDispatchers();

            return services;
        }
    }
}
