using Autofac;
using CQELight.Abstractions.IoC.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.IoC.Autofac
{
    internal class AutofacScopeFactory : IScopeFactory
    {
        #region Members

        private readonly ILifetimeScope _rootScope;

        #endregion

        #region Static properties

        /// <summary>
        /// Current instance.
        /// </summary>
        internal static AutofacScopeFactory Instance;

        #endregion

        #region Ctor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="autofacContainer">Autofac container.</param>
        public AutofacScopeFactory(ILifetimeScope autofacContainer)
        {
            _rootScope = autofacContainer ?? throw new ArgumentNullException(nameof(autofacContainer),
                "AutofacScopeFactory.ctor() : Autofac container should be provided.");
            Instance = this;
        }

        #endregion

        #region IScopeFactory methods

        /// <summary>
        /// Create a new scope.
        /// </summary>
        /// <returns>New instance of scope.</returns>
        public IScope CreateScope() => new AutofacScope(_rootScope.BeginLifetimeScope());

        #endregion

    }
}
