using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Dispatcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQELight.TestFramework
{
    /// <summary>
    /// Assertion upon synchronous actions.
    /// </summary>
    public class TestFrameworkActionAssertion
    {
        #region Members

        private readonly Action _action;
        private static readonly SemaphoreSlim s_Semaphore = new SemaphoreSlim(1);

        #endregion

        #region Ctor

        internal TestFrameworkActionAssertion(Action action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action), "TestFrameworkAssertion.ctor() : Action for assertion must be specified.");
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Check that no events are raised by executing the action.
        /// </summary>
        /// <param name="timeout">Timeout.</param>
        public void ThenNoEventShouldBeRaised(ulong timeout = 1000)
        {
            s_Semaphore.Wait();
            var events = new List<IDomainEvent>();
            try
            {
                var lambda = new Func<IDomainEvent, Task>((e) =>
                {
                    events.Add(e);
                    return Task.CompletedTask;
                });
                CoreDispatcher.OnEventDispatched += lambda;
                try
                {
                    Task.Run(() => _action.Invoke(), new CancellationTokenSource((int)timeout).Token)
                        .GetAwaiter().GetResult();
                }
                finally
                {
                    CoreDispatcher.OnEventDispatched -= lambda;
                }
            }
            finally
            {
                s_Semaphore.Release();
            }
            if (events.Any())
            {
                throw new TestFrameworkException($"No events where expected, however, events of type [" +
                    $"{string.Join($"{Environment.NewLine},", events.Select(e => e.GetType().FullName))} " +
                    $"] where dispatched.");
            }
        }

        /// <summary>
        /// Check that no commands are dispatched by executing the action.
        /// </summary>
        /// <param name="timeout">Tiemout.</param>
        public void ThenNoCommandAreDispatched(ulong timeout = 1000)
        {
            s_Semaphore.Wait();
            var commands = new List<ICommand>();
            try
            {
                var lambda = new Func<ICommand, Task>(c => { commands.Add(c); return Task.CompletedTask; });
                CoreDispatcher.OnCommandDispatched += lambda;
                try
                {
                    Task.Run(() => _action.Invoke(), new CancellationTokenSource((int)timeout).Token)
                        .GetAwaiter().GetResult();
                }
                finally
                {
                    CoreDispatcher.OnCommandDispatched -= lambda;
                }
            }
            finally
            {
                s_Semaphore.Release();
            }
            if (commands.Any())
            {
                throw new TestFrameworkException($"No commands where expected, however commands of type [" +
                    $"{string.Join($"{Environment.NewLine},", commands.Select(e => e.GetType().FullName))} " +
                    $"] where dispatched.");
            }
        }

        /// <summary>
        /// Check that a specific event type instance has been dispatched in the system.
        /// </summary>
        /// <typeparam name="T">Expected event type</typeparam>
        /// <param name="timeout">Timeout.</param>
        public T ThenEventShouldBeRaised<T>(ulong timeout = 1000) where T : class, IDomainEvent
        {
            s_Semaphore.Wait();
            T @event = null;
            try
            {
                var lambda = new Func<IDomainEvent, Task>(e =>
                {
                    if (e is T)
                    {
                        @event = e as T;
                    }
                    return Task.CompletedTask;
                });
                CoreDispatcher.OnEventDispatched += lambda;
                try
                {
                    Task.Run(() => _action.Invoke(), new CancellationTokenSource((int)timeout).Token);
                }
                finally
                {
                    CoreDispatcher.OnEventDispatched -= lambda;
                }
            }
            finally
            {
                s_Semaphore.Release();
            }

            if (@event == null)
            {
                throw new TestFrameworkException($"L'évenement de type {typeof(T).Name} attendu par l'exécution d'une action n'a pas été dispatché.");
            }

            return @event;
        }

        /// <summary>
        /// Check that a bunch of events are raised in the system.
        /// </summary>
        /// <param name="timeout">Timeout.</param>
        public IEnumerable<IDomainEvent> ThenEventsShouldBeRaised(ulong timeout = 1000)
        {
            s_Semaphore.Wait();
            var events = new List<IDomainEvent>();
            try
            {
                var lambda = new Func<IDomainEvent, Task>((e) =>
                {
                    events.Add(e);
                    return Task.CompletedTask;
                });
                CoreDispatcher.OnEventDispatched += lambda;
                try
                {
                    Task.Run(() => _action.Invoke(), new CancellationTokenSource((int)timeout).Token);
                }
                finally
                {
                    CoreDispatcher.OnEventDispatched -= lambda;
                }
            }
            finally
            {
                s_Semaphore.Release();
            }
            if (!events.Any())
            {
                throw new TestFrameworkException($"No events were dispatched, but some were excepted.");
            }

            return events;
        }

        /// <summary>
        /// Check that a bunch of commands are dispatch in the system.
        /// </summary>
        /// <param name="tiemout">Timeout.</param>
        /// <returns>All dispatched commands, if any.</returns>
        public IEnumerable<ICommand> ThenCommandsAreDispatched(ulong tiemout = 100, bool autoFakeHandlers = true)
        {
            s_Semaphore.Wait();
            var commands = new List<ICommand>();
            try
            {
                var lambda = new Func<ICommand, Task>(c => { commands.Add(c); return Task.CompletedTask; });
                CoreDispatcher.OnCommandDispatched += lambda;
                try
                {
                    Task.Run(() => _action.Invoke(), new CancellationTokenSource((int)tiemout).Token);
                }
                finally
                {
                    CoreDispatcher.OnCommandDispatched -= lambda;
                }
            }
            finally
            {
                s_Semaphore.Release();
            }
            if (commands?.Any() == false)
            {
                throw new TestFrameworkException("No command were dispatched, but some were expected.");
            }
            return commands;
        }

        /// <summary>
        /// Check that a specific command type instance is dispatched in the system.
        /// </summary>
        /// <typeparam name="T">Command type.</typeparam>
        /// <param name="timeout">Timeout.</param>
        /// <returns>Dispatched command.</returns>
        public T ThenCommandIsDispatched<T>(ulong timeout = 1000) where T : class, ICommand
        {
            s_Semaphore.Wait();
            T command = null;
            try
            {
                var lambda = new Func<ICommand, Task>(c =>
                {
                    if (c is T)
                    {
                        command = c as T;
                    }
                    return Task.CompletedTask;
                });
                CoreDispatcher.OnCommandDispatched += lambda;
                try
                {
                    Task.Run(() => _action.Invoke(), new CancellationTokenSource((int)timeout).Token);
                }
                finally
                {
                    CoreDispatcher.OnCommandDispatched -= lambda;
                }
            }
            finally
            {
                s_Semaphore.Release();
            }
            if (command == null)
            {
                throw new TestFrameworkException($"Command of type {typeof(T).Name} was expected, but wasn't dispatched.");
            }
            return command;
        }

        #endregion

    }
}
