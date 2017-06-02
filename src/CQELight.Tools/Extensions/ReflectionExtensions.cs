using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;

namespace CQELight.Tools
{
    /// <summary>
    /// Extension methods for reflection.
    /// </summary>
    public static class ReflectionExtensions
    {

        #region Extension methods
        
        /// <summary>
        /// Retrieve all types of current app.
        /// </summary>
        /// <returns>Collection of types.</returns>
        public static IEnumerable<Type> GetAllTypes(string currentPath)
        {
#if NET452
            var dependencies = AppDomain.CurrentDomain.GetAssemblies();
            return dependencies.SelectMany(a => a.GetTypes());
#else
            var dependencies = DependencyContext.Default.RuntimeLibraries;
            List<Type> result = new List<Type>();
            foreach (var library in dependencies.Where(d => d.Type.Equals("project", StringComparison.OrdinalIgnoreCase)))
            {
                if (File.Exists(Path.Combine(Path.GetDirectoryName(currentPath), library.RuntimeAssemblyGroups[0].AssetPaths[0])))
                {
                    var assembly = Assembly.Load(new AssemblyName(library.Name));
                    result.AddRange(assembly.GetTypes());
                }

            }
            return result;
#endif

        }

        #endregion

    }
}
