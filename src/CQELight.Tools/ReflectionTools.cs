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
        private static ConcurrentBag<Type> s_AllTypes = new ConcurrentBag<Type>();
        /// <summary>
        /// List of already treated assemblies.
        /// </summary>
        private static ConcurrentBag<string> s_LoadedAssemblies = new ConcurrentBag<string>();

        /// <summary>
        /// List of DLL to not load for types.
        /// </summary>
        private static readonly string[] CONST_REJECTED_DLLS = new[] { "Microsoft", "System", "sqlite3" };
        /// <summary>
        /// Thread safety init object.
        /// </summary>
        private static object s_Lock = new object();

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
        /// Get all types from current app by looking to all associated DLLs
        /// </summary>
        /// <param name="rejectedDlls">Name of DLL to no inspect</param>
        /// <returns>Collection of all app types.</returns>
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
                    s_AllTypes = new ConcurrentBag<Type>();
                }
            }
            var initialCount = s_AllTypes.Count;
            var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var rejectedDLLs = CONST_REJECTED_DLLS.Concat(rejectedDlls);
            if (domainAssemblies != null)
            {
                domainAssemblies.DoForEach(a =>
                {
                    if (!rejectedDLLs.Any(s => a.GetName().Name.StartsWith(s))
                    && !s_LoadedAssemblies.Contains(a.GetName().Name))
                    {
                        s_LoadedAssemblies.Add(a.GetName().Name);
                        Type[] types = new Type[0];
                        try
                        {
                            types = a.GetTypes();
                        }
                        catch (ReflectionTypeLoadException e)
                        {
                            types = e.Types.WhereNotNull().ToArray();
                        }
                        catch
                        {
                            //Ignore all others exceptions
                        }
                        for (int i = 0; i < types.Length; i++)
                        {
                            s_AllTypes.Add(types[i]);
                        }
                    }
                }, true);
            }
            var assemblies = new DirectoryInfo(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)).GetFiles("*.dll", SearchOption.AllDirectories);
            if (assemblies.Length > 0)
            {
                assemblies.DoForEach(file =>
                {
                    if (!rejectedDLLs.Any(s => file.Name.StartsWith(s, StringComparison.OrdinalIgnoreCase))
                     && !s_LoadedAssemblies.Contains(file.FullName))
                    {
                        s_LoadedAssemblies.Add(file.FullName);
                        Type[] types = new Type[0];
                        var a = new Proxy().GetAssembly(file.FullName);
                        if (a != null && !AppDomain.CurrentDomain.GetAssemblies().Any(assembly => assembly.GetName() == a.GetName()))
                        {
#pragma warning disable S3885 // "Assembly.Load" should be used
                            AppDomain.CurrentDomain.Load(Assembly.LoadFrom(file.FullName).GetName());
#pragma warning restore S3885 // "Assembly.Load" should be used
                            try
                            {
                                types = a.GetTypes();
                            }
                            catch (ReflectionTypeLoadException e)
                            {
                                types = e.Types.WhereNotNull().ToArray();
                            }
                            catch
                            {
                                //Ignore all others exception
                            }
                            for (int i = 0; i < types.Length; i++)
                            {
                                s_AllTypes.Add(types[i]);
                            }
                        }
                    }
                }, true);
            }
            if (s_AllTypes.Count != initialCount)
            {
                lock (s_Lock)
                {
                    s_AllTypes = new ConcurrentBag<Type>(s_AllTypes.Distinct(new TypeEqualityComparer()));
                }

            }
            return s_AllTypes;
        }
        #endregion
    }
}
