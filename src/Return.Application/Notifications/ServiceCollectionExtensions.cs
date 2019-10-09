// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ServiceCollectionExtensions.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal static class ServiceCollectionExtensions {
        public static void AddNotificationDispatcher<TNotificationDispatcher>(this IServiceCollection services) {
            Type dispatcher = typeof(TNotificationDispatcher);
            AddNotificationDispatcher(services: services, dispatcher);
        }

        public static void AddNotificationDispatcher(this IServiceCollection services, Type dispatcherType) {
            Type? dispatcherBase = dispatcherType.BaseType;

            if (dispatcherBase == null || dispatcherBase.GetGenericTypeDefinition() != typeof(NotificationDispatcher<,>)) {
                throw new InvalidOperationException(
                    $"The type {dispatcherType} does not inherit from {typeof(NotificationDispatcher<,>)}");
            }

            // Register services:
            object ImplementationFactory(IServiceProvider sp) => sp.GetService(dispatcherType);

            // 1. The dispatcher itself
            services.AddSingleton(dispatcherType);

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

        public static void AddNotificationDispatchers(this IServiceCollection serviceCollection) {
            IEnumerable<Type> allTypes =
                from type in Assembly.GetExecutingAssembly().GetExportedTypes()
                where !type.IsAbstract
                where type.BaseType != null && type.BaseType.IsConstructedGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(NotificationDispatcher<,>)
                select type;

            foreach (Type dispatcherType in allTypes) {
                Debug.WriteLine($"Registering {dispatcherType} as dispatcher");

                serviceCollection.AddNotificationDispatcher(dispatcherType);
            }
        }
    }
}
