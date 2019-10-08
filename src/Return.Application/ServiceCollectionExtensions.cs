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
    using Notifications.RetrospectiveStatusUpdated;
    using Retrospectives.Queries.GetRetrospectiveStatus;
    using RetrospectiveWorkflows.Common;

    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddApplication(this IServiceCollection services) {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddMediatR(opts => opts.AsScoped(), Assembly.GetExecutingAssembly());

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestPerformanceBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));

            services.AddScoped<IRetrospectiveStatusMapper, RetrospectiveStatusMapper>();
            services.AddScoped<IRetrospectiveStatusUpdateDispatcher, RetrospectiveStatusUpdateDispatcher>();

            services.AddNotificationDispatcher<RetrospectiveJoinedNotificationDispatcher>();
            services.AddNotificationDispatcher<RetrospectiveStatusUpdatedNotificationDispatcher>();

            return services;
        }
    }
}
