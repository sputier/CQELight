using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Implementations.IoC;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.IoC.Microsoft.Extensions.DependencyInjection
{
    class MicrosoftScope : IScope
    {

        #region Members

        private readonly IServiceCollection _services;
        private readonly ServiceProvider _serviceProvider;

        #endregion

        #region Ctor

        public MicrosoftScope(IServiceCollection services)
        {
            _services = services;
            _serviceProvider = services.BuildServiceProvider();
        }

        #endregion

        #region IScope methods

        public bool IsDisposed => true;

        public IScope CreateChildScope(Action<ITypeRegister> typeRegisterAction = null)
        {
            var childrenCollection = _services.Clone();
            if (typeRegisterAction != null)
            {
                var typeRegister = new TypeRegister();
                typeRegisterAction(typeRegister);
                MicrosoftRegistrationHelper.RegisterContextTypes(childrenCollection, typeRegister);
            }

            return new MicrosoftScope(childrenCollection);
        }

        public void Dispose()
        {
        }

        public T Resolve<T>(params IResolverParameter[] parameters) where T : class
        {
            if (parameters.Length > 0)
            {
                throw new NotSupportedException("Microsoft.Extensions.DependencyInjection doesn't officially supports parameters injection during runtime. You should register parameters retrieving via a factory or change to another IoC container provider that supports parameters injection at runtime.");
            }
            return _serviceProvider.GetService<T>();
        }

        public object Resolve(Type type, params IResolverParameter[] parameters)
        {
            return _serviceProvider.GetService(type);
        }

        public IEnumerable<T> ResolveAllInstancesOf<T>() where T : class => _serviceProvider.GetServices<T>();

        public IEnumerable ResolveAllInstancesOf(Type t) => _serviceProvider.GetServices(t);


        #endregion

    }
}
