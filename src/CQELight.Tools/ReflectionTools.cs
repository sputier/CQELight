using CQELight.Tools.Extensions;
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

        #region Static members

        /// <summary>
        /// All current types
        /// </summary>
        [ThreadStatic]
        internal static List<Type> s_AllTypes = new List<Type>();
        /// <summary>
        /// List of already treated assemblies.
        /// </summary>
        [ThreadStatic]
        internal static ConcurrentBag<string> s_LoadedAssemblies = new ConcurrentBag<string>();

        /// <summary>
        /// List of DLL to not load for types.
        /// </summary>
        private static string[] CONST_REJECTED_DLLS = new[] { "Microsoft", "System", "sqlite3" };

        #endregion

        #region Nested class

        /// <summary>
        /// Proxy class to load assembly by reflection without adding it to AppDomain in full Framework.
        /// </summary>
        private class Proxy : MarshalByRefObject
        {
            /// <summary>
            /// Retrieve an assembly from it's path.
            /// </summary>
            /// <param name="assemblyPath">Path to the assembly.</param>
            /// <returns>Assembly loaded.</returns>
            public Assembly GetAssembly(string assemblyPath)
            {
                try
                {
                    if (File.Exists(assemblyPath))
                    {
                        return Assembly.LoadFile(assemblyPath);
                    }
                }
                finally { }
                return null;

            }
        }

        #endregion

        #region Extension methods

        /// <summary>
        /// Retrieve all types of current app.
        /// </summary>
        /// <returns>Collection of types.</returns>
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
            if (DependencyContext.Default != null) // .NETCoreApp
            {
                var dependencies = DependencyContext.Default.RuntimeLibraries;
                foreach (var library in dependencies.Where(d => d.Type.Equals("project", StringComparison.OrdinalIgnoreCase)))
                {
                    if (File.Exists(Path.Combine(Path.GetDirectoryName(Environment.CurrentDirectory), library.RuntimeAssemblyGroups[0].AssetPaths[0])))
                    {
                        var assembly = Assembly.Load(new AssemblyName(library.Name));
                        s_AllTypes.AddRange(assembly.GetTypes());
                    }
                }
            }
            else //Compatibility with full Framework
            {

                void LoadAssemblyTypes(Assembly assembly)
                {
                    try
                    {
                        s_AllTypes.AddRange(assembly.GetTypes());
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        s_AllTypes.AddRange(e.Types.Where(t => t != null));
                    }
                    catch { }
                }

                var assemblies = new DirectoryInfo(Environment.CurrentDirectory).GetFiles("*.dll", SearchOption.AllDirectories);
                if (assemblies.Any())
                {
                    assemblies.DoForEach(file =>
                    {
                        s_LoadedAssemblies.Add(file.FullName);
                        if (!CONST_REJECTED_DLLS.Any(s => file.Name.StartsWith(s, StringComparison.OrdinalIgnoreCase))
                         && !s_LoadedAssemblies.Contains(file.FullName))
                        {
                            var assembly = new Proxy().GetAssembly(file.FullName);
                            if (assembly != null)
                            {
                                LoadAssemblyTypes(assembly);
                            }
                        }
                    });
                    var domainAssemblies = AppDomain.CurrentDomain?.GetAssemblies();
                    if (domainAssemblies != null)
                    {
                        domainAssemblies.DoForEach(a =>
                        {
                            s_LoadedAssemblies.Add(a.FullName);
                            if (!CONST_REJECTED_DLLS.Any(s => a.GetName().Name.StartsWith(s)
                            && !s_LoadedAssemblies.Contains(a.GetName().Name)))
                            {
                                LoadAssemblyTypes(a);
                            }
                        });
                    }
                }
            }
            return s_AllTypes;
        }

        #endregion
    }
}
