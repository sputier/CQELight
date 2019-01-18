using Autofac;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using A = Autofac;

namespace CQELight.IoC.Autofac
{
    internal class AutoRegisterModule : A.Module
    {

        #region Members

        private readonly string[] _excludedAutoRegisterTypeDlls;

        #endregion

        #region Ctor

        public AutoRegisterModule(params string[] excludedAutoRegisterTypeDlls)
        {
            _excludedAutoRegisterTypeDlls = (excludedAutoRegisterTypeDlls ?? Enumerable.Empty<string>()).Concat(new[] { "Autofac" }).ToArray();
        }

        #endregion

        #region Override methods
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            foreach (var type in ReflectionTools.GetAllTypes(_excludedAutoRegisterTypeDlls)
                .Where(t => typeof(IAutoRegisterType).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract).ToList())
            {
                builder.RegisterType(type)
                    .IfNotRegistered(type)
                    .AsImplementedInterfaces()
                    .AsSelf()
                    .FindConstructorsWith(new FullConstructorFinder());
            }

            foreach (var type in ReflectionTools.GetAllTypes(_excludedAutoRegisterTypeDlls)
                .Where(t => typeof(IAutoRegisterTypeSingleInstance).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract).ToList())
            {
                builder.RegisterType(type)
                    .IfNotRegistered(type)
                    .AsImplementedInterfaces()
                    .AsSelf()
                    .SingleInstance()
                    .FindConstructorsWith(new FullConstructorFinder());
            }
        }

        #endregion

    }
}
