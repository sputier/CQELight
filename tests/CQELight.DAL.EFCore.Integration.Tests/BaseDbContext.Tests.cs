using CQELight.TestFramework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.DAL.EFCore.Integration.Tests
{
    public class BaseDbContextTests : BaseUnitTestClass
    {

        #region Ctor & members

        #endregion

        #region MigrateAsync

        [Fact]
        public async Task BaseDbContext_MigrateAsync_MultipleBases()
        {
            if(File.Exists("specific.db"))
            {
                File.Delete("specific.db");
            }
            if(File.Exists("general.db"))
            {
                File.Delete("general.db");
            }
            var specificOptions = new DbContextOptionsBuilder()
                .UseSqlite("Data Source=specific.db");
            var generalOptions = new DbContextOptionsBuilder()
                .UseSqlite("Data Source=general.db");
            using (var ctx = new TestDbContext(generalOptions.Options))
            {
                await ctx.Database.MigrateAsync();
            }

            using (var ctx = new TestDbContext(specificOptions.Options))
            {
                await ctx.Database.MigrateAsync();
            }
        }


        #endregion

    }
}
