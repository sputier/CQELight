using Autofac;
using Autofac.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using CQELight.Abstractions.Infrastructure;
using System.Text;
using MoreLinq;

namespace CQELight.Implementations.Infrastructure
{
    /// <summary>
    /// Implementation of DI management with Autofac
    /// </summary>
    public class AutofacDIManager : ITypeResolver
    {

        #region Members

        /// <summary>
        /// Scope of Autofac
        /// </summary>
        readonly ILifetimeScope _scope;

        #endregion

        #region Ctor

        /// <summary>
        /// Default constructor
        /// </summary>
        public AutofacDIManager(ILifetimeScope scope)
        {
            _scope = scope ?? throw new ArgumentNullException(nameof(scope), "AutofacDIManager.ctor () : Scope cannot be null");
        }

        #endregion

        #region ITypeResolver methods

        /// <summary>
        /// Resolve an instance of type T.
        /// </summary>
        /// <typeparam name="T">Type to resolve.</typeparam>
        /// <returns>Instance of T</returns>
        public T Resolve<T>(params TypeResolverParameter[] parameters)
             => _scope.Resolve<T>(GetParams(parameters));

        /// <summary>
        /// Resolve an instance of asked type.
        /// </summary>
        /// <param name="type">Type to resolve.</param>
        /// <returns>Instance of type</returns>
        public object Resolve(Type type, params TypeResolverParameter[] parameters)
             => _scope.Resolve(type, GetParams(parameters));

        #endregion

        #region Private methods

        /// <summary>
        /// Retrieve Autofac parameters from CQELight parameters.
        /// </summary>
        /// <param name="parameters">CQELigh list of parameters.</param>
        /// <returns>Autofac parameters</returns>
        private Parameter[] GetParams(TypeResolverParameter[] parameters)
        {
            List<Parameter> @params = new List<Parameter>();
            if (parameters != null && parameters.Any())
            {
                parameters.ForEach(p => @params.Add(new NamedParameter(p.Name, p.Value)));
            }

            return @params.ToArray();
        }


        #endregion
    }
}
