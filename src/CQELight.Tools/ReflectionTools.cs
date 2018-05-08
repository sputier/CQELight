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
        private static List<Type> s_AllTypes = new List<Type>();
        /// <summary>
        /// List of already treated assemblies.
        /// </summary>
        private static ConcurrentBag<string> s_LoadedAssemblies = new ConcurrentBag<string>();

        /// <summary>
        /// List of DLL to not load for types.
        /// </summary>
        private static readonly string[] CONST_REJECTED_DLLS = new[] { "Microsoft", "System", "sqlite3" };
        /// <summary>
        /// Thread safety object.
        /// </summary>
        private static readonly object s_Lock = new object();

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
#pragma warning disable S3885 // "Assembly.Load" should be used
                        return Assembly.LoadFile(assemblyPath);
#pragma warning restore S3885 // "Assembly.Load" should be used
                    }
                }
                catch
                {
                    //If file is not loadable, just return null
                }
                return null;
            }
        }

        #endregion

        #region Public static methods

        /// <summary>
        /// Retournes tous les types chargés dans le programme courant.
        /// </summary>
        /// <returns>Collection de type.</returns>
        public static IEnumerable<Type> GetAllTypes(params string[] rejectedDlls)
        {
            lock (s_Lock)
            {
                if (s_LoadedAssemblies == null)
                {
                    s_LoadedAssemblies = new ConcurrentBag<string>();
                }
                if (s_AllTypes == null)
                {
                    s_AllTypes = new List<Type>();
                }
            }

            var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (domainAssemblies != null)
            {
                domainAssemblies.DoForEach(a =>
                {
                    if (!CONST_REJECTED_DLLS.Any(s => a.GetName().Name.StartsWith(s))
                    && !s_LoadedAssemblies.Contains(a.GetName().Name))
                    {
                        s_LoadedAssemblies.Add(a.GetName().Name);
                        try
                        {
                            lock (s_Lock)
                            {
                                s_AllTypes.AddRange(a.GetTypes());
                            }
                        }
                        catch (ReflectionTypeLoadException e)
                        {
                            lock (s_Lock)
                            {
                                s_AllTypes.AddRange(e.Types.Where(t => t != null));
                            }
                        }
                        catch
                        {
                            //Ignore all others exceptions
                        }
                    }
                });
            }
            var assemblies = new DirectoryInfo(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)).GetFiles("*.dll", SearchOption.AllDirectories);
            if (assemblies.Any())
            {
                assemblies.DoForEach(file =>
                {
                    if (!CONST_REJECTED_DLLS.Any(s => file.Name.StartsWith(s, StringComparison.OrdinalIgnoreCase))
                     && !s_LoadedAssemblies.Contains(file.FullName))
                    {
                        s_LoadedAssemblies.Add(file.FullName);
                        var a = new Proxy().GetAssembly(file.FullName);
                        if (a != null && !AppDomain.CurrentDomain.GetAssemblies().Any(assembly => assembly.GetName() == a.GetName()))
                        {
#pragma warning disable S3885 // "Assembly.Load" should be used
                            AppDomain.CurrentDomain.Load(Assembly.LoadFrom(file.FullName).GetName());
#pragma warning restore S3885 // "Assembly.Load" should be used
                            try
                            {
                                lock (s_Lock)
                                {
                                    s_AllTypes.AddRange(a.GetTypes());
                                }
                            }
                            catch (ReflectionTypeLoadException e)
                            {
                                lock (s_Lock)
                                {
                                    s_AllTypes.AddRange(e.Types.Where(t => t != null));
                                }
                            }
                            catch
                            {
                                //Ignore all others exception
                            }
                        }
                    }
                });
            }
            lock (s_Lock)
            {
                s_AllTypes = s_AllTypes.Distinct(new TypeEqualityComparer()).ToList();
            }
            return s_AllTypes;
        }
        #endregion
    }
}
