using Autofac;
using CQELight.Abstractions.IoC.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Implementations.IoC.Autofac
{
    /// <summary>
    /// ScopeFactory for Autofac scopes.
    /// </summary>
    public class AutofacScopeFactory : IScopeFactory
    {

        #region Members

        /// <summary>
        /// Container autofac
        /// </summary>
        readonly IContainer _container;

        #endregion

        #region Ctor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="autofacContainer">Autofac container.</param>
        public AutofacScopeFactory(IContainer autofacContainer)
        {
            _container = autofacContainer ?? throw new ArgumentNullException(nameof(autofacContainer),
                "AutofacScopeFactory.ctor() : Autofac container should be provided.");
        }

        #endregion

        #region IScopeFactory methods

        /// <summary>
        /// Création d'un scope autofac.
        /// </summary>
        /// <returns>Scope DI.</returns>
        public IScope CreateScope()
        {
            Action<ContainerBuilder> autoRegisterAction = s => s.RegisterModule<AutoRegisterModule>();
            return new AutofacScope(_container.BeginLifetimeScope(autoRegisterAction));
        }

        #endregion

    }
}
