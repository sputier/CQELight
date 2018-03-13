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

        readonly Action _action;
        static SemaphoreSlim s_Semaphore = new SemaphoreSlim(1);

        #endregion

        #region Ctor

        /// <summary>
        /// Create a new TestFrameworkAssertion upon a synchronous action.
        /// </summary>
        /// <param name="action">Action to invoke for assertion.</param>
        public TestFrameworkActionAssertion(Action action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action), "TestFrameworkAssertion.ctor() : Action for assertion must be specified.");
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Check that no events are raised by the following action.
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
            if (events.Any())
            {
                throw new TestFrameworkException($"No events where expected, however, events of type [" +
                    $"{string.Join($"{Environment.NewLine},", events.Select(e => e.GetType().FullName))} " +
                    $"] where dispatched.");
            }
        }

        /// <summary>
        /// Vérifie qu'aucun évènement n'est lancé suite à l'exécution d'une action
        /// </summary>
        public void ThenNoCommandAreDispatched(ulong waitTime = 1000)
        {
            s_Semaphore.Wait();
            var commands = new List<ICommand>();
            try
            {
                var lambda = new Action<ICommand>(commands.Add);
                CoreDispatcher.OnCommandDispatched += lambda;
                try
                {
                    Task.Run(() => _action.Invoke(), new CancellationTokenSource((int)waitTime).Token);
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
                throw new TestFrameworkException($"Aucun commande n'étaient attendues, pourtant le(s) commande(s) de type [" +
                    $"{string.Join($"{Environment.NewLine},", commands.Select(e => e.GetType().FullName))} " +
                    $"]a/ont été dispatché(s).");
            }
        }

        /// <summary>
        /// Vérifie que l'évenement de type voulu est bien lancé dans le système.
        /// </summary>
        /// <typeparam name="T">Type d'évenement attendu.</typeparam>
        /// <param name="waitTime">Temps d'attente maximum en millisecondes.</param>
        public T ThenEventShouldBeRaised<T>(ulong waitTime = 1000) where T : class, IDomainEvent
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
                    Task.Run(() => _action.Invoke(), new CancellationTokenSource((int)waitTime).Token);
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
        /// Vérifie que plusieurs évenements sont publiés dans le système.
        /// </summary>
        /// <param name="waitTime">Temps d'attente maximum en millisecondes.</param>
        public IEnumerable<IDomainEvent> ThenEventsShouldBeRaised(ulong waitTime = 1000)
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
                    Task.Run(() => _action.Invoke(), new CancellationTokenSource((int)waitTime).Token);
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
                throw new TestFrameworkException($"Aucun évenements n'a été publié suite à l'action attendue.");
            }

            return events;
        }

        /// <summary>
        /// Vérifies qu'un ensemble de commandes sont dispatchées .
        /// </summary>
        /// <typeparam name="T">Type de commande à dispatcher.</typeparam>
        /// <param name="waitTime">Temps d'attente maximum en millisecondes.</param>
        /// <returns>Collection de commandes dispatchées.</returns>
        public IEnumerable<ICommand> ThenCommandsAreDispatched(ulong waitTime = 100, bool autoFakeHandlers = true)
        {
            s_Semaphore.Wait();
            var commands = new List<ICommand>();
            try
            {
                var lambda = new Action<ICommand>(c =>
                {
                    commands.Add(c);
                });
                CoreDispatcher.OnCommandDispatched += lambda;
                try
                {
                    Task.Run(() => _action.Invoke(), new CancellationTokenSource((int)waitTime).Token);
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
                throw new TestFrameworkException("Aucune commande dispatchées l'exécution d'une action.");
            }
            return commands;
        }
        /// <summary>
        /// Vérifie que l'invocation d'une action dispatche une commande dans le système.
        /// </summary>
        /// <typeparam name="T">Type de commande à dispatcher.</typeparam>
        /// <param name="waitTime">Temps d'attente maximum en millisecondes.</param>
        /// <returns>Instance de la commande dispatchée.</returns>
        public T ThenCommandIsDispatched<T>(ulong waitTime = 100, bool autoFakeHandlers = true) where T : class, ICommand
        {
            s_Semaphore.Wait();
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
                    Task.Run(() => _action.Invoke(), new CancellationTokenSource((int)waitTime).Token);
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
                throw new TestFrameworkException($"La command de type {typeof(T).Name} attendu par l'exécution d'une action n'a pas été dispatchée.");
            }
            return command;
        }

        #endregion

    }
}
