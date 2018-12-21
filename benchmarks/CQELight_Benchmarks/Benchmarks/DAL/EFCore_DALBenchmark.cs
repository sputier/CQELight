using BenchmarkDotNet.Attributes;
using CQELight.DAL.EFCore;
using CQELight_Benchmarks.Benchmarks.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQELight_Benchmarks.Benchmarks.DAL
{
    public class EFCore_DALBenchmark : BaseBenchmark
    {

        #region BenchmarkDotNet

        [Params(DatabaseType.SQLite, DatabaseType.SQLServer)]
        public DatabaseType DatabaseType;

        [Params(10, 100, 1000)]
        public int NbIterations;

        [GlobalSetup]
        public void GlobalSetup()
        {
            CreateDatabase();

        }

        [IterationSetup]
        public void IterationSetup()
        {
            CleanDatabases();
        }

        private void CleanDatabases()
        {
            using (var ctx = new EFCoreBenchmarkDbContext(GetOptions()))
            {
                ctx.RemoveRange(ctx.Set<Hyperlink>());
                ctx.RemoveRange(ctx.Set<WebSite>());
                ctx.RemoveRange(ctx.Set<Post>());
                ctx.RemoveRange(ctx.Set<Comment>());
                ctx.RemoveRange(ctx.Set<User>());
                ctx.SaveChanges();
            }
        }

        private void CreateDatabase()
        {
            using (var ctx = new EFCoreBenchmarkDbContext(GetOptions()))
            {
                ctx.Database.EnsureDeleted();
                ctx.Database.EnsureCreated();
            }
        }

        private DbContextOptions GetOptions()
        {
            switch (DatabaseType)
            {
                case DatabaseType.SQLite: return new DbContextOptionsBuilder().UseSqlite("Filename=EFCoreBenchmarks.db").Options;
                default: return new DbContextOptionsBuilder().UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=CQELight_EFCoreBenchmarks").Options;
            }
        }

        #endregion

        #region Benchmarks

        [Benchmark]
        public async Task InsertSimple()
        {
            using (var repo = new EFRepository<WebSite>(new EFCoreBenchmarkDbContext(GetOptions())))
            {
                for (int i = 0; i < NbIterations; i++)
                {
                    var website = new WebSite
                    {
                        Url = "http://blogs.msdn.com/dotnet/" + i
                    };
                    repo.MarkForInsert(website);
                }
                await repo.SaveAsync().ConfigureAwait(false);
            }
        }

        [Benchmark]
        public async Task InsertComplex()
        {
            using (var repo = new EFRepository<WebSite>(new EFCoreBenchmarkDbContext(GetOptions())))
            {
                for (int i = 0; i < NbIterations; i++)
                {
                    var website = new WebSite
                    {
                        Url = "http://blogs.msdn.com/dotnet/" + i,
                        AzureLocation = new AzureLocation
                        {
                            Country = "FR",
                            DataCenter = "Marseille"
                        },
                        HyperLinks = new List<Hyperlink>
                        {
                            new Hyperlink
                            {
                                Value = "http://blogs.msd.com/dotnet/article/" + i
                            }
                        },
                        Posts = new List<Post>
                        {
                            new Post
                            {
                                Content = string.Concat(Enumerable.Repeat("Content ", 50)),
                                QuickUrl = "http://bit.ly/" + i
                            }
                        }
                    };
                    repo.MarkForInsert(website);
                }
                await repo.SaveAsync().ConfigureAwait(false);
            }
        }

        #endregion

    }
}
