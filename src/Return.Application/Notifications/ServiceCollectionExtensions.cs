// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ServiceCollectionExtensions.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications {
    using System;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    internal static class ServiceCollectionExtensions {
        public static void AddNotificationDispatcher<TNotificationDispatcher>(this IServiceCollection services) {
            Type dispatcher = typeof(TNotificationDispatcher);
            Type? dispatcherBase = dispatcher.BaseType;

            if (dispatcherBase == null || dispatcherBase.GetGenericTypeDefinition() != typeof(NotificationDispatcher<,>)) {
                throw new InvalidOperationException($"The type {dispatcher} does not inherit from {typeof(NotificationDispatcher<,>)}");
            }

            // Register services:
            object ImplementationFactory(IServiceProvider sp) => sp.GetService(dispatcher);

            // 1. The dispatcher itself
            services.AddSingleton(dispatcher);

            // 2. Its base class
            services.AddSingleton(dispatcherBase, ImplementationFactory);

            // 3. The abstract INotificationSubscription<TSubscriber> interface
            services.AddSingleton(
                typeof(INotificationSubscription<>).MakeGenericType(dispatcherBase.GenericTypeArguments[1]),
                ImplementationFactory);

            // 4. Mediator interface
            Type notificationHandlerType =
                typeof(INotificationHandler<>).MakeGenericType(dispatcherBase.GenericTypeArguments[0]);
            services.RemoveAll(notificationHandlerType);
            services.AddSingleton(notificationHandlerType, ImplementationFactory);
        }
    }
}
