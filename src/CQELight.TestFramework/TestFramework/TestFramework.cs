using CQELight.Abstractions.Events.Interfaces;
using CQELight.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CQELight.TestFramework
{
    /// <summary>
    /// Helping class to manage test functions upon
    /// </summary>
    public static class Test
    {

        #region Static members

        /// <summary>
        /// Thread safety object.
        /// </summary>
        static SemaphoreSlim s_lock = new SemaphoreSlim(1);

        #endregion

        #region Public static methods

        /// <summary>
        /// Création d'une assertion de commande. 
        /// Cette assertion permet de jauger le résultat de l'exécution d'une commande.
        /// </summary>
        /// <typeparam name="T">Type de commande à instancier.</typeparam>
        /// <param name="parameters">Parameters pour instancier la commande.</param>
        /// <returns>Assertion à vérifier.</returns>
        public static TestFrameworkCommandAssertion WhenCommand<T>(params object[] parameters)
            where T : ICommand
        {
            var commandInstance = (ICommand)typeof(T).CreateInstance(parameters);
            if (commandInstance == null)
            {
                throw new InvalidOperationException($"TestFramework.WhenCommand() : Impossible de créer une commande de type {typeof(T).Name}.");
            }
            return new TestFrameworkCommandAssertion(commandInstance);
        }

        /// <summary>
        /// Création d'une assertion de commande avec une commande définie.
        /// </summary>
        /// <param name="command">Commande de l'assertion.</param>
        /// <returns>Assertion à vérifier.</returns>
        public static TestFrameworkCommandAssertion WhenCommand(ICommand command)
            => new TestFrameworkCommandAssertion(command);


        /// <summary>
        /// Création d'une assertion d'action avec une action définie.
        /// </summary>
        /// <param name="act">Action définie.</param>
        /// <returns>Assertion à vérifier.</returns>
        public static TestFrameworkActionAssertion When(Action act)
            => new TestFrameworkActionAssertion(act);
        /// <summary>
        /// Création d'une assertion d'action avec une action définie.
        /// </summary>
        /// <param name="act">Action définie.</param>
        /// <returns>Assertion à vérifier.</returns>
        public static TestFrameworkAsyncActionAssertion WhenAsync(Func<Task> act)
            => new TestFrameworkAsyncActionAssertion(act);

        /// <summary>
        /// Création d'une assertion sur ViewModel.
        /// </summary>
        /// <param name="vm">View Model à asserter.</param>
        /// <returns>Assertion à vérifier.</returns>
        public static TestFrameworkViewModelAssertion<T> OnViewModel<T>(T vm)
            where T : BaseViewModel
            => new TestFrameworkViewModelAssertion<T>(vm);

        #endregion

    }
}
