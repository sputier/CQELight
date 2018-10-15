using CQELight.TestFramework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.DAL.EFCore.Integration.Tests
{
    public class BaseDbContextTests : BaseUnitTestClass
    {

        #region Ctor & members

        #endregion

        #region Nested classes

        private class GeneralConfiguration : IDatabaseContextConfigurator
        {
            public void ConfigureConnectionString(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlite("Data Source=general.db");
            }
        }

        private class SpecificConfiguration : IDatabaseContextConfigurator
        {
            public void ConfigureConnectionString(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlite("Data Source=specific.db");
            }
        }

        #endregion

        #region MigrateAsync

        [Fact]
        public async Task BaseDbContext_MigrateAsync_MultipleBases()
        {
            using (var ctx = new TestDbContext(new GeneralConfiguration()))
            {
                await ctx.Database.MigrateAsync();
            }

            using (var ctx = new TestDbContext(new SpecificConfiguration()))
            {
                await ctx.Database.MigrateAsync();
            }
        }


        #endregion

    }
}
