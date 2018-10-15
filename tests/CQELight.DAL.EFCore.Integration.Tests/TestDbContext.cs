using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace CQELight.DAL.EFCore.Integration.Tests
{
    public class TestDatabaseContextConfigurator : IDatabaseContextConfigurator
    {
        public void ConfigureConnectionString(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TDD_Base;Trusted_Connection=True;MultipleActiveResultSets=true;");
        }
    }

    public class TestDbContext : BaseDbContext
    {
        public TestDbContext()
            : base(new TestDatabaseContextConfigurator())
        {
        }

        public TestDbContext(IDatabaseContextConfigurator databaseContextConfigurator)
            : base(databaseContextConfigurator)
        { }
    }
}
