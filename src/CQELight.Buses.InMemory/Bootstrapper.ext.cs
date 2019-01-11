using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Bootstrapping.Notifications;
using CQELight.Buses.InMemory;
using CQELight.Buses.InMemory.Commands;
using CQELight.Buses.InMemory.Events;
using CQELight.Dispatcher;
using CQELight.IoC;
using CQELight.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight
{
    public static class BootstrapperExt
    {

        #region Private static members

        private static List<Type> s_AllTypes;
        private static List<Type> _allTypes
        {
            get
            {
                if (s_AllTypes == null)
                {
                    s_AllTypes = ReflectionTools.GetAllTypes().ToList();
                }
                return s_AllTypes;
            }
        }

        #endregion

        #region Public static methods

        /// <summary>
        /// Configure the bootstrapper to use InMemory buses for dispatching events.
        /// </summary>
        /// <param name="bootstrapper">Instance of boostrapper.</param>
        /// <param name="configuration">Configuration to use for in memory event bus.</param>
        /// <param name="excludedEventsDLLs">DLLs name to exclude from auto-configuration into IoC
        /// (IAutoRegisterType will be ineffective).</param>
        public static Bootstrapper UseInMemoryEventBus(this Bootstrapper bootstrapper, InMemoryEventBusConfiguration configuration = null,
                    params string[] excludedEventsDLLs)
        {
            var service = InMemoryBusesBootstrappService.Instance;
            service.BootstrappAction += (ctx) =>
            {
                InMemoryEventBus.InitHandlersCollection(excludedEventsDLLs);
                if (ctx.IsServiceRegistered(BootstrapperServiceType.IoC))
                {
                    bootstrapper.AddIoCRegistration(new TypeRegistration(typeof(InMemoryEventBus), typeof(IDomainEventBus), typeof(InMemoryEventBus)));
                    if (configuration != null)
                    {
                        bootstrapper.AddIoCRegistration(new InstanceTypeRegistration(configuration, typeof(InMemoryEventBusConfiguration)));
                    }
                }
                bootstrapper.AddNotifications(PerformEventChecksAccordingToBootstrapperParameters(ctx, configuration));
            };
            if (!bootstrapper.RegisteredServices.Any(s => s == service))
            {
                bootstrapper.AddService(service);
            }
            return bootstrapper;
        }

        /// <summary>
        /// Configure the system to use InMemory Event bus for dipsatching events, with the provided configuration.
        /// </summary>
        /// <param name="bootstrapper">Bootstrapper instance.</param>
        /// <param name="configurationBuilderAction">Action to apply on builder.</param>
        /// <param name="excludedEventsDLLs">DLLs name to exclude from auto-configuration into IoC
        /// (IAutoRegisterType will be ineffective).</param>
        /// <returns>Bootstrapper Instance.</returns>
        public static Bootstrapper UseInMemoryEventBus(this Bootstrapper bootstrapper, Action<InMemoryEventBusConfigurationBuilder> configurationBuilderAction,
            params string[] excludedEventsDLLs)
        {
            if (configurationBuilderAction == null)
                throw new ArgumentNullException(nameof(configurationBuilderAction));

            InMemoryEventBus.InitHandlersCollection(excludedEventsDLLs);
            var builder = new InMemoryEventBusConfigurationBuilder();
            configurationBuilderAction(builder);

            return UseInMemoryEventBus(bootstrapper, builder.Build());
        }

        /// <summary>
        /// Configure the bootstrapper to use InMemory buses for dispatching commands.
        /// </summary>
        /// <param name="bootstrapper">Instance of boostrapper.</param>
        /// <param name="configuration">Configuration to use for in memory command bus.</param>
        /// <param name="excludedCommandsDLLs">DLLs name to exclude from auto-configuration into IoC
        /// (IAutoRegisterType will be ineffective).</param>
        public static Bootstrapper UseInMemoryCommandBus(this Bootstrapper bootstrapper, InMemoryCommandBusConfiguration configuration = null,
            params string[] excludedCommandsDLLs)
        {
            var service = InMemoryBusesBootstrappService.Instance;
            service.BootstrappAction += (ctx) =>
            {
                InMemoryCommandBus.InitHandlersCollection(excludedCommandsDLLs);
                if (ctx.IsServiceRegistered(BootstrapperServiceType.IoC))
                {
                    bootstrapper.AddIoCRegistration(new TypeRegistration(typeof(InMemoryCommandBus), typeof(ICommandBus), typeof(InMemoryCommandBus)));
                    if (configuration != null)
                    {
                        bootstrapper.AddIoCRegistration(new InstanceTypeRegistration(configuration, typeof(InMemoryCommandBusConfiguration)));
                    }
                }
                bootstrapper.AddNotifications(PerformCommandChecksAccordingToBootstrapperParameters(ctx, configuration));
            };
            if (!bootstrapper.RegisteredServices.Any(s => s == service))
            {
                bootstrapper.AddService(service);
            }
            return bootstrapper;
        }

        /// <summary>
        /// Configure the system to use InMemory Command bus for dipsatching commands, with the provided configuration.
        /// </summary>
        /// <param name="bootstrapper">Bootstrapper instance.</param>
        /// <param name="configurationBuilderAction">Action to apply on builder.</param>
        /// <param name="excludedCommandsDLLs">DLLs name to exclude from auto-configuration into IoC
        /// (IAutoRegisterType will be ineffective).</param>
        /// <returns>Bootstrapper Instance.</returns>
        public static Bootstrapper UseInMemoryCommandBus(this Bootstrapper bootstrapper, Action<InMemoryCommandBusConfigurationBuilder> configurationBuilderAction,
            params string[] excludedCommandsDLLs)
        {
            if (configurationBuilderAction == null)
                throw new ArgumentNullException(nameof(configurationBuilderAction));

            InMemoryCommandBus.InitHandlersCollection(excludedCommandsDLLs);
            var builder = new InMemoryCommandBusConfigurationBuilder();
            configurationBuilderAction(builder);

            return UseInMemoryCommandBus(bootstrapper, builder.Build());
        }

        #endregion

        #region Private static methods

        private static IEnumerable<BootstrapperNotification> PerformCommandChecksAccordingToBootstrapperParameters(BootstrappingContext ctx,
            InMemoryCommandBusConfiguration configuration)
        {
            var notifs = new List<BootstrapperNotification>();
            if (ctx.CheckOptimal)
            {
                foreach (var cmdType in _allTypes.Where(t => typeof(ICommand).IsAssignableFrom(t) && t != typeof(ICommand)).AsParallel())
                {
                    if (_allTypes.Count(t => typeof(ICommandHandler<>).MakeGenericType(cmdType).IsAssignableFrom(t)) == 0)
                    {
                        notifs.Add(
                            new BootstrapperNotification(
                                BootstrapperNotificationType.Warning,
                                $"Your project doesn't contains any handler for command type {cmdType.FullName}, which is generally not desired (you forget to add implementation of it). When implementing it, don't forget to allow InMemoryCommandBus to be able to retrieve it (by adding it to your IoC container or by creating a parameterless constructor to allow to create it by reflection). If this is desired, you can ignore this warning.",
                                typeof(InMemoryCommandBus))
                            );
                    }
                }
            }
            if (ctx.Strict)
            {
                foreach (var cmdType in _allTypes.Where(t => typeof(ICommand).IsAssignableFrom(t)).AsParallel())
                {
                    if (_allTypes.Count(t => typeof(ICommandHandler<>).MakeGenericType(cmdType).IsAssignableFrom(t)) > 1)
                    {
                        if (ctx.CheckOptimal)
                        {
                            notifs.Add(
                                new BootstrapperNotification(
                                    BootstrapperNotificationType.Error,
                                    $"Your project contains more than one handler for command type {cmdType.FullName}. This is not allowed by best practices, even if you configured it in configuration, because you've configured your bootstrapper to be 'strict' and 'optimal'. To get rid of this error, remove handlers until you get only one left, or change your bootstrapper configuration.",
                                    typeof(InMemoryCommandBus))
                                );
                        }
                        else
                        {
                            notifs.Add(
                                new BootstrapperNotification(
                                    BootstrapperNotificationType.Warning,
                                    $"Your project contains more than one handler for command type {cmdType.FullName}. This is generally a bad practice, even if you configured it in configuration. It is recommended to have only one handler per command type to avoid multiple treatment of same command. This warning can be remove by passing false to bootstrapper for the 'strict' flag.",
                                    typeof(InMemoryCommandBus))
                                );
                        }
                    }
                }
            }
            return notifs.AsEnumerable();
        }

        private static IEnumerable<BootstrapperNotification> PerformEventChecksAccordingToBootstrapperParameters(BootstrappingContext ctx,
            InMemoryEventBusConfiguration configuration)
        {
            var notifs = new List<BootstrapperNotification>();
            if (ctx.CheckOptimal)
            {
                foreach (var evtType in _allTypes.Where(t => typeof(IDomainEvent).IsAssignableFrom(t) && t != typeof(IDomainEvent)).AsParallel())
                {
                    if (_allTypes.Count(t => typeof(IDomainEventHandler<>).MakeGenericType(evtType).IsAssignableFrom(t)) == 0)
                    {
                        notifs.Add(
                            new BootstrapperNotification(
                                BootstrapperNotificationType.Warning,
                                $"Your project doesn't contains any handler for event type {evtType.FullName}, which is generally not desired (you forget to add implementation of it). When implementing it, don't forget to allow InMemoryEventBus to be able to retrieve it (by adding it to your IoC container or by creating a parameterless constructor to allow to create it by reflection). If this is desired, you can ignore this warning.",
                                typeof(InMemoryEventBus))
                            );
                    }
                }
            }
            return notifs.AsEnumerable();
        }

        #endregion

    }
}
