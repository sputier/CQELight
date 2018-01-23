using Autofac;
using Autofac.Core;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.IoC;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Implementations.IoC.Autofac
{
    /// <summary>
    /// Scope implementation for Autofac.
    /// </summary>
    public class AutofacScope : IScope
    {

        #region Members

        /// <summary>
        /// Container Autofac
        /// </summary>
        readonly ILifetimeScope scope;
        /// <summary>
        /// Identifiant unique du scope.
        /// </summary>
        public Guid Id { get; protected set; }

        #endregion

        #region Properties
        
        /// <summary>
        /// Indicates if scope is disposed or not.
        /// </summary>
        public bool IsDisposed { get; private set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="scope">Autofac lifetime scope.</param>
        internal AutofacScope(ILifetimeScope scope)
        {
            this.scope = scope;
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~AutofacScope()
        {
            Dispose(false);
        }

        #endregion

        #region IScope methods
        
        /// <summary>
        /// Create a whole new scope with all current's scope registration.
        /// </summary>
        /// <param name="typeRegisterAction">Specific child registration..</param>
        /// <returns>Child scope.</returns>
        public IScope CreateChildScope(Action<ITypeRegister> typeRegisterAction = null)
        {
            Action<ContainerBuilder> act = null;
            if (typeRegisterAction != null)
            {
                var typeRegister = new TypeRegister();
                typeRegisterAction.Invoke(typeRegister);
                act += b => AutofacTools.RegisterContextTypes(b, typeRegister);
            }
            if (act != null)
            {
                return new AutofacScope(scope.BeginLifetimeScope(act));
            }
            return new AutofacScope(scope.BeginLifetimeScope());
        }

        #endregion

        #region IDisposable methods

        /// <summary>
        /// Cleaning up resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region ITypeResolver
        
        /// <summary>
        /// Resolve instance of type.
        /// </summary>
        /// <typeparam name="T">Instance of type we want to resolve.</typeparam>
        /// <param name="parameters">Parameters for resolving.</param>
        /// <returns></returns>
        public T Resolve<T>(params IResolverParameter[] parameters) where T : class
            => scope.ResolveOptional<T>(GetParams(parameters));
        
        /// <summary>
        /// Resolve instance of type.
        /// </summary>
        /// <param name="type">Type we want to resolve instance.</param>
        /// <param name="parameters">Parameters for resolving.</param>
        /// <returns>Instance of resolved type.</returns>
        public object Resolve(Type type, params IResolverParameter[] parameters)
            => scope.ResolveOptional(type, GetParams(parameters));

        /// <summary>
        /// Retrieve all instances of a specific type from IoC container.
        /// </summary>
        /// <typeparam name="T">Excepted types.</typeparam>
        /// <returns>Collection of implementations for type.</returns>
        public IEnumerable<T> ResolveAllInstancesOf<T>() where T : class
            => scope.ResolveOptional<IEnumerable<T>>();

        #endregion

        #region Private methods

        /// <summary>
        /// Cleaning up resources.
        /// </summary>
        /// <param name="dispose">Flag to indicates if we come from dispose.</param>
        private void Dispose(bool dispose)
        {
            IsDisposed = true;
            scope.Dispose();
            if (dispose)
            {
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Create Autofac parameters from IResolverParameters.
        /// </summary>
        /// <param name="parameters">Paremeters.</param>
        /// <returns>Collection of Autofac parameters.</returns>
        private IEnumerable<Parameter> GetParams(IResolverParameter[] parameters)
        {
            var @params = new List<Parameter>();
            foreach (var par in parameters)
            {
                if (par is NameResolverParameter namePar)
                {
                    @params.Add(new NamedParameter(namePar.Name, namePar.Value));
                }
                else if (par is TypeResolverParameter typePar)
                {
                    @params.Add(new TypedParameter(typePar.Type, typePar.Value));
                }

            }
            return @params;
        }

        #endregion
    }
}
