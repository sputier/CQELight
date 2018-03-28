using CQELight.DAL.EFCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Examples.Console
{
    class AppDbContextConfigurator : IDatabaseContextConfigurator
    {
        public void ConfigureConnectionString(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TestApp_Base;Trusted_Connection=True;MultipleActiveResultSets=true;");
        }
    }

    public class AppDbContext : BaseDbContext
    {
        public AppDbContext()
            : base(new AppDbContextConfigurator())
        {
        }
    }
}
