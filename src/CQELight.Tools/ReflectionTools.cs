using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CQELight.Tools
{
    /// <summary>
    /// Bunch of tool that are linked to reflection.
    /// </summary>
    public static class ReflectionTools
    {

        #region Members

        /// <summary>
        /// All current types.
        /// </summary>
        internal static List<Type> s_AllTypes = new List<Type>();
        /// <summary>
        /// All already loaded assemblies.
        /// </summary>
        internal static ConcurrentBag<string> s_LoadedAssemblies = new ConcurrentBag<string>();

        #endregion

        #region Public static methods

        /// <summary>
        /// Retrieve all the types from all assemblies of current app.
        /// </summary>
        /// <returns>Collectoin of types.</returns>
        public static IEnumerable<Type> GetAllTypes()
        {
            if (s_LoadedAssemblies == null)
            {
                s_LoadedAssemblies = new ConcurrentBag<string>();
            }
            if (s_AllTypes == null)
            {
                s_AllTypes = new List<Type>();
            }
#if NET462
            var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (domainAssemblies.Any())
            {
                domainAssemblies.DoForEach(a =>
                {
                    if (!s_LoadedAssemblies.Contains(a.FullName))
                    {
                        s_LoadedAssemblies.Add(a.FullName);
                        try
                        {
                            s_AllTypes.AddRange(a.GetTypes());
                        }
                        catch { }
                    }
                });
            }

            return s_AllTypes;
#else
            var dependencies = DependencyContext.Default.RuntimeLibraries;

            foreach (var library in dependencies.Where(d => d.Type.Equals("project", StringComparison.OrdinalIgnoreCase)))
            {
                if (File.Exists(Path.Combine(Path.GetDirectoryName(typeof(ReflectionTools).GetTypeInfo().Assembly.Location),
                    library.RuntimeAssemblyGroups[0].AssetPaths[0])))
                {
                    if (!s_LoadedAssemblies.Contains(library.Name))
                    {
                        s_LoadedAssemblies.Add(library.Name);
                        var assembly = Assembly.Load(new AssemblyName(library.Name));
                        s_AllTypes.AddRange(assembly.GetTypes());
                    }
                }

            }
            return s_AllTypes;
#endif

        }

        #endregion

    }
}
