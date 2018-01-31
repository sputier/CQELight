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
    /// <summary>
    /// Module autofac pour l'enregistrement auto des types
    /// </summary>
    public class AutoRegisterModule : A.Module
    {

        #region Override methods

        /// <summary>
        /// Chargement des types dans le container.
        /// </summary>
        /// <param name="builder">Builder de container</param>
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            foreach (var type in ReflectionTools.GetAllTypes().Where(t => typeof(IAutoRegisterType).IsAssignableFrom(t)))
            {
                builder.RegisterType(type)
                    .IfNotRegistered(type)
                    .AsImplementedInterfaces()
                    .AsSelf();
            }
        }

        #endregion

    }
}
