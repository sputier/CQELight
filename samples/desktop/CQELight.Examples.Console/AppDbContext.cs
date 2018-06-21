using CQELight.DAL.EFCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Examples.Console
{
    internal static class Consts
    {
        public const string CONST_CONNECTION_STRING = "Server=(localdb)\\mssqllocaldb;Database=TestApp_Base;Trusted_Connection=True;MultipleActiveResultSets=true;";
        public const string CONST_EVENT_DB_CONNECTION_STRING = "Server=(localdb)\\mssqllocaldb;Database=TestApp_Events_Base;Trusted_Connection=True;MultipleActiveResultSets=true;";
    }

    internal class AppDbContextConfigurator : IDatabaseContextConfigurator
    {
        public void ConfigureConnectionString(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Consts.CONST_CONNECTION_STRING);
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
