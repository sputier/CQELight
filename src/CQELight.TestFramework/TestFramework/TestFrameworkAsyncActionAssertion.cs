using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Dispatcher;
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
    /// Assertion upon an asynchronous action.
    /// </summary>
    public class TestFrameworkAsyncActionAssertion
    {
        #region Members

        private readonly Func<Task> _action;
        private static SemaphoreSlim s_lock { get; } = new SemaphoreSlim(1);

        #endregion

        #region Ctor

        internal TestFrameworkAsyncActionAssertion(Func<Task> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action), "TestFrameworkAsyncActionAssertion.ctor() : Action to invoke must be provided.");
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Assert that no events are raised by the action.
        /// </summary>
        /// <param name="timeout">Timeout.</param>
        public async Task ThenNoEventShouldBeRaised(ulong timeout = 1000)
        {
            await s_lock.WaitAsync().ConfigureAwait(false);
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
                    await Task.Run(_action.Invoke, new CancellationTokenSource((int)timeout).Token).ConfigureAwait(false);
                }
                finally
                {
                    CoreDispatcher.OnEventDispatched -= lambda;
                }
            }
            finally
            {
                s_lock.Release();
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
        public async Task ThenNoCommandAreDispatched(ulong timeout = 1000)
        {
            await s_lock.WaitAsync().ConfigureAwait(false);
            var commands = new List<ICommand>();
            try
            {
                var lambda = new Action<ICommand>(commands.Add);
                CoreDispatcher.OnCommandDispatched += lambda;
                try
                {
                    await Task.Run(_action.Invoke, new CancellationTokenSource((int)timeout).Token).ConfigureAwait(false);
                }
                finally
                {
                    CoreDispatcher.OnCommandDispatched -= lambda;
                }
            }
            finally
            {
                s_lock.Release();
            }

            if (commands.Any())
            {
                throw new TestFrameworkException($"No commands where expected, however commands of type [" +
                    $"{string.Join($"{Environment.NewLine},", commands.Select(e => e.GetType().FullName))} " +
                    $"] where dispatched.");
            }
        }

        /// <summary>
        /// Check that no messages were raised on an async action.
        /// </summary>
        /// <param name="timeout">Timeout.</param>
        public async Task ThenNoMessageShouldBeRaised(ulong timeout = 1000)
        {
            await s_lock.WaitAsync().ConfigureAwait(false);
            var appMessages = new List<IMessage>();
            try
            {
                var lambda = new Action<IMessage>((e) =>
                {
                    appMessages.Add(e);
                });
                CoreDispatcher.OnMessageDispatched += lambda;
                try
                {
                    await Task.Run(_action.Invoke, new CancellationTokenSource((int)timeout).Token).ConfigureAwait(false);
                }
                finally
                {
                    CoreDispatcher.OnMessageDispatched -= lambda;
                }
            }
            finally
            {
                s_lock.Release();
            }
            if (appMessages.Any())
            {
                throw new TestFrameworkException("No messages were expected, however message(s) of type [" +
                    $"{string.Join($"{Environment.NewLine},", appMessages.Select(e => e.GetType().FullName))} " +
                    $"] was/were dispatched.");
            }
        }

        /// <summary>
        /// Check that a specific event type instance has been dispatched in the system.
        /// </summary>
        /// <typeparam name="T">Expected event type</typeparam>
        /// <param name="timeout">Timeout.</param>
        public async Task<T> ThenEventShouldBeRaised<T>(ulong waitTime = 1000) where T : class, IDomainEvent
        {
            await s_lock.WaitAsync().ConfigureAwait(false);
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
                    await Task.Run(_action.Invoke, new CancellationTokenSource((int)waitTime).Token).ConfigureAwait(false);
                }
                finally
                {
                    CoreDispatcher.OnEventDispatched -= lambda;
                }
            }
            finally
            {
                s_lock.Release();
            }
            if (@event == null)
            {
                throw new TestFrameworkException($"Event of type {typeof(T).Name} hasn't been dispatched before timeout.");
            }
            return @event;
        }

        /// <summary>
        /// Check that a bunch of events are raised in the system.
        /// </summary>
        /// <param name="timeout">Timeout.</param>
        public async Task<IEnumerable<IDomainEvent>> ThenEventsShouldBeRaised(ulong waitTime = 1000)
        {
            await s_lock.WaitAsync().ConfigureAwait(false);
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
                    await Task.Run(_action.Invoke, new CancellationTokenSource((int)waitTime).Token).ConfigureAwait(false);
                }
                finally
                {
                    CoreDispatcher.OnEventDispatched -= lambda;
                }
            }
            finally
            {
                s_lock.Release();
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
        public async Task<IEnumerable<ICommand>> ThenCommandsAreDispatched(ulong waitTime = 1000, bool autoFakeHandlers = true)
        {
            await s_lock.WaitAsync().ConfigureAwait(false);
            var commands = new List<ICommand>();
            try
            {
                var lambda = new Action<ICommand>(c => commands.Add(c));
                CoreDispatcher.OnCommandDispatched += lambda;
                try
                {
                    await Task.Run(() => _action.Invoke(), new CancellationTokenSource((int)waitTime).Token).ConfigureAwait(false);
                }
                finally
                {
                    CoreDispatcher.OnCommandDispatched -= lambda;
                }
            }
            finally
            {
                s_lock.Release();
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
        public async Task<T> ThenCommandIsDispatched<T>(ulong waitTime = 1000) where T : class, ICommand
        {
            await s_lock.WaitAsync().ConfigureAwait(false);
            T command = null;
            try
            {
                var lambda = new Action<ICommand>(c =>
                {
                    if (c is T)
                    {
                        command = c as T;
                    }
                });
                CoreDispatcher.OnCommandDispatched += lambda;
                try
                {
                    await Task.Run(() => _action.Invoke(), new CancellationTokenSource((int)waitTime).Token).ConfigureAwait(false);
                }
                finally
                {
                    CoreDispatcher.OnCommandDispatched -= lambda;
                }
            }
            finally
            {
                s_lock.Release();
            }
            if (command == null)
            {
                throw new TestFrameworkException($"Command of type {typeof(T).Name} was expected, but wasn't dispatched.");
            }
            return command;
        }

        /// <summary>
        /// Check that a specific IMessage type has been raised within the system.
        /// </summary>
        /// <typeparam name="T">Type of expected IMessage.</typeparam>
        /// <param name="timeout">Timeout.</param>
        public async Task<T> ThenMessageShouldBeRaised<T>(ulong timeout = 1000) where T : class, IMessage
        {
            s_lock.Wait();
            T appMessage = null;
            try
            {
                var lambda = new Action<IMessage>(e =>
                {
                    if (e is T)
                    {
                        appMessage = e as T;
                    }
                });
                CoreDispatcher.OnMessageDispatched += lambda;
                try
                {
                    await Task.Run(() => _action.Invoke(), new CancellationTokenSource((int)timeout).Token).ConfigureAwait(false);
                }
                finally
                {
                    CoreDispatcher.OnMessageDispatched -= lambda;
                }
            }
            finally
            {
                s_lock.Release();
            }

            if (appMessage == null)
            {
                throw new TestFrameworkException($"Messages of type {typeof(T).Name} not raised before timeout.");
            }

            return appMessage;
        }

        /// <summary>
        /// Check that the CoreDispatcher has received some messages.
        /// </summary>
        /// <param name="timeout">Timeout.</param>
        public async Task<IEnumerable<IMessage>> ThenMessagesShouldBeRaised(ulong timeout = 1000)
        {
            s_lock.Wait();
            var appMessages = new List<IMessage>();
            try
            {
                var lambda = new Action<IMessage>((e) => appMessages.Add(e));
                CoreDispatcher.OnMessageDispatched += lambda;
                try
                {
                    await Task.Run(() => _action.Invoke(), new CancellationTokenSource((int)timeout).Token).ConfigureAwait(false);
                }
                finally
                {
                    CoreDispatcher.OnMessageDispatched -= lambda;
                }
            }
            finally
            {
                s_lock.Release();
            }
            if (appMessages.Count == 0)
            {
                throw new TestFrameworkException("No messages dispatched within the system.");
            }

            return appMessages;
        }


        #endregion
    }
}
