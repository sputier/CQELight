using CQELight.DAL.Common;
using CQELight.DAL.Interfaces;
using CQELight.IoC;
using CQELight.Tools;
using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQELight.DAL.EFCore
{
    public static class BootstrapperExt
    {

        #region Private static members

        private static Dictionary<string, Type> s_ContextTypesPerAssembly
            = new Dictionary<string, Type>();


        #endregion

        #region Public static methods

        /// <summary>
        /// Configure EF Core as repository implementation. 
        /// This methods uses a single DbContext for all repositories
        /// </summary>
        /// <param name="bootstrapper">Bootstrapper instance</param>
        /// <param name="dbContext">Instance of BaseDbContext to use</param>
        public static Bootstrapper UseEFCoreAsMainRepository(this Bootstrapper bootstrapper, BaseDbContext dbContext)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            var entities = ReflectionTools.GetAllTypes().Where(t => t.IsSubclassOf(typeof(BaseDbEntity))).ToList();
            foreach (var item in entities)
            {
                var efRepoType = typeof(EFRepository<>).MakeGenericType(item);
                var dataReaderRepoType = typeof(IDataReaderRepository<>).MakeGenericType(item);
                var databaseRepoType = typeof(IDatabaseRepository<>).MakeGenericType(item);
                var dataUpdateRepoType = typeof(IDataUpdateRepository<>).MakeGenericType(item);

                bootstrapper
                    .AddIoCRegistration(new FactoryRegistration(() => efRepoType.CreateInstance(dbContext),
                        dataUpdateRepoType, databaseRepoType, dataReaderRepoType));
            }
            return bootstrapper;
        }

        /// <summary>
        /// Configure EF Core as repository implementation. 
        /// This methods uses a single database configuration and create dynamically all context
        /// from every concerned assembly.
        /// </summary>
        /// <param name="bootstrapper">Bootstrapper instance</param>
        /// <param name="dbConfiguration">Configuration to use</param>
        public static Bootstrapper UseEFCoreAsMainRepository(this Bootstrapper bootstrapper, IDatabaseContextConfigurator dbConfiguration)
        {
            if (dbConfiguration == null)
            {
                throw new ArgumentNullException(nameof(dbConfiguration));
            }

            foreach (var item in ReflectionTools.GetAllTypes().Where(t => t.IsSubclassOf(typeof(BaseDbEntity))).ToList())
            {
                var efRepoType = typeof(EFRepository<>).MakeGenericType(item);
                var dataReaderRepoType = typeof(IDataReaderRepository<>).MakeGenericType(item);
                var databaseRepoType = typeof(IDatabaseRepository<>).MakeGenericType(item);
                var dataUpdateRepoType = typeof(IDataUpdateRepository<>).MakeGenericType(item);

                Type ctxType = null;

                if (s_ContextTypesPerAssembly.ContainsKey(item.Assembly.FullName))
                {
                    ctxType = s_ContextTypesPerAssembly[item.Assembly.FullName];
                }
                else
                {
                    ctxType = null;
                    s_ContextTypesPerAssembly[item.Assembly.FullName] = ctxType;
                }

                if (ctxType == null)
                {
                    throw new InvalidOperationException("Bootstrapper.UseEFCoreAsMainRepository() : " +
                        $"No DbContext found for assembly {item.Assembly.FullName}, but this assembly contains a " +
                        "some persistence entities. You need to create a specific class that inherits from BaseDbContext in this assembly to use this configuration method.");
                }

                bootstrapper
                    .AddIoCRegistration(new FactoryRegistration(() =>
                    {
                        var ctx = ctxType.CreateInstance(dbConfiguration);
                        return efRepoType.CreateInstance(ctx);
                    }, dataUpdateRepoType, databaseRepoType, dataReaderRepoType));
            }
            return bootstrapper;
        }

        #endregion

    }
}