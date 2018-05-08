using Autofac;
using CQELight.Implementations.IoC;
using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.IoC.Autofac
{
    internal static class AutofacTools
    {
        #region static methods

        /// <summary>
        /// Handling to typeRegister objects to inject their data into Autofac ContainerBuilder
        /// </summary>
        /// <param name="b">Autofac's ContainerBuilder.</param>
        /// <param name="typeRegister">TypeRegister instance.</param>
        public static void RegisterContextTypes(ContainerBuilder b, TypeRegister typeRegister)
        {
            typeRegister.Objects.DoForEach(o =>
            {
                if (o != null)
                {
                    b.RegisterInstance(o)
                       .AsImplementedInterfaces()
                       .AsSelf();
                }
            });
            typeRegister.Types.DoForEach(t =>
            {
                if (t != null)
                {
                    b.RegisterType(t)
                       .AsImplementedInterfaces()
                       .AsSelf()
                       .FindConstructorsWith(new FullConstructorFinder());
                }
            });
            typeRegister.ObjAsTypes.DoForEach(kvp =>
            {
                if (kvp.Key != null)
                {
                    b.RegisterInstance(kvp.Key)
                       .As(kvp.Value)
                       .AsImplementedInterfaces()
                       .AsSelf();
                }
            });
            typeRegister.TypeAsTypes.DoForEach(kvp =>
            {
                if (kvp.Key != null)
                {
                    b.RegisterType(kvp.Key)
                       .As(kvp.Value)
                       .AsImplementedInterfaces()
                       .AsSelf()
                       .FindConstructorsWith(new FullConstructorFinder());
                }
            });
        }

        #endregion
    }
}
