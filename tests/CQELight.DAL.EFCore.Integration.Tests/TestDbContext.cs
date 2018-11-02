using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace CQELight.DAL.EFCore.Integration.Tests
{

    public class TestDbContext : BaseDbContext
    {
        public TestDbContext()
            : base(new DbContextOptionsBuilder().UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TDD_Base;Trusted_Connection=True;MultipleActiveResultSets=true;").Options)
        {
        }

        public TestDbContext(DbContextOptions options)
            : base(options)
        { }
    }
}
