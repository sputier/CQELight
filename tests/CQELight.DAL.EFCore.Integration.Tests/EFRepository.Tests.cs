using CQELight.TestFramework;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.DAL.EFCore.Integration.Tests
{

    #region Nested class

    class TestBlogEFRepository : EFRepository<WebSite>
    {
        public TestBlogEFRepository() : base(new TestDbContext())
        {
        }

        public WebSite GetWithPostAndComment()
            => DataSet
                .Include(b => b.Posts)
                .ThenInclude(p => p.Writer)
                .ThenInclude(u => u.Comments)
                .First();
    }

    #endregion

    public class EFRepositoryTests : BaseUnitTestClass
    {

        #region Ctor & members

        private static bool _isInit;

        public EFRepositoryTests()
        {
            if (!_isInit)
            {
                using (var ctx = new TestDbContext())
                {
                    ctx.Database.EnsureDeleted();
                    ctx.Database.Migrate();
                }
                _isInit = true;
            }
            DeleteAll();
        }

        private void DeleteAll()
        {
            using (var ctx = new TestDbContext())
            {
                ctx.RemoveRange(ctx.Set<Hyperlink>());
                ctx.RemoveRange(ctx.Set<WebSite>());
                ctx.SaveChanges();
            }
        }

        #endregion

        #region Get

        [Fact]
        public async Task EFRepository_SimpleGet_NoFilter_NoOrder_NoIncludes_Should_Returns_All()
        {
            try
            {
                using (var ctx = new TestDbContext())
                {
                    ctx.Add(new WebSite
                    {
                        Url = "https://blogs.msdn.net"
                    });
                    ctx.Add(new WebSite
                    {
                        Url = "https://www.microsoft.com"
                    });
                    await ctx.SaveChangesAsync();
                }

                using (var repo = new TestBlogEFRepository())
                {
                    var sites = await repo.GetAsync().ToList();
                    sites.Should().HaveCount(2);
                    sites.Any(s => s.Url.Contains("msdn")).Should().BeTrue();
                    sites.Any(s => s.Url.Contains("microsoft")).Should().BeTrue();
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        [Fact]
        public async Task EFRepository_SimpleGet_WithFilter_NoOrder_NoIncludes_Should_Returns_FilteredItemsOnly()
        {
            try
            {
                using (var ctx = new TestDbContext())
                {
                    ctx.Add(new WebSite
                    {
                        Url = "https://blogs.msdn.net"
                    });
                    ctx.Add(new WebSite
                    {
                        Url = "https://www.microsoft.com"
                    });
                    await ctx.SaveChangesAsync();
                }

                using (var repo = new TestBlogEFRepository())
                {
                    var sites = await repo.GetAsync(w => w.Url.Contains("msdn")).ToList();
                    sites.Should().HaveCount(1);
                    sites.Any(s => s.Url.Contains("msdn")).Should().BeTrue();
                    sites.Any(s => s.Url.Contains("microsoft")).Should().BeFalse();
                }
            }
            finally
            {
                DeleteAll();
            }
        }


        [Fact]
        public async Task EFRepository_SimpleGet_NoFilter_NohOrder_NoIncludes_WithDeleted_Should_GetAll()
        {
            try
            {
                using (var ctx = new TestDbContext())
                {
                    ctx.Add(new WebSite
                    {
                        Url = "https://blogs.msdn.net"
                    });
                    ctx.Add(new WebSite
                    {
                        Url = "https://www.microsoft.com",
                        Deleted = true,
                        DeletionDate = DateTime.Now
                    });
                    await ctx.SaveChangesAsync();
                }

                using (var repo = new TestBlogEFRepository())
                {
                    var sites = await repo.GetAsync(includeDeleted: true).ToList();
                    sites.Should().HaveCount(2);
                    sites.Any(s => s.Url.Contains("msdn")).Should().BeTrue();
                    sites.Any(s => s.Url.Contains("microsoft")).Should().BeTrue();
                    sites.Any(s => s.Deleted).Should().BeTrue();
                    sites.Any(s => !s.Deleted).Should().BeTrue();
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        [Fact]
        public async Task EFRepository_SimpleGet_NoFilter_WithOrder_NoIncludes_Should_Respect_Order()
        {
            try
            {
                using (var ctx = new TestDbContext())
                {
                    ctx.Add(new WebSite
                    {
                        Url = "https://blogs.msdn.net"
                    });
                    ctx.Add(new WebSite
                    {
                        Url = "https://www.microsoft.com"
                    });
                    await ctx.SaveChangesAsync();
                }

                using (var repo = new TestBlogEFRepository())
                {
                    var sites = await repo.GetAsync(orderBy: b => b.Url).ToList();
                    sites.Should().HaveCount(2);
                    sites.Any(s => s.Url.Contains("msdn")).Should().BeTrue();
                    sites.Any(s => s.Url.Contains("microsoft")).Should().BeTrue();
                    sites.First().Url.Should().Contain("msdn");
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        [Fact]
        public async Task EFRepository_SimpleGet_NoFilter_NoOrder_WithIncludes_Should_GetLinkedData()
        {
            try
            {
                using (var ctx = new TestDbContext())
                {
                    ctx.Add(new WebSite
                    {
                        Url = "https://blogs.msdn.net",
                        HyperLinks = new List<Hyperlink>
                        {
                            new Hyperlink
                            {
                                Value = "https://blogs.msdn.net"
                            },
                            new Hyperlink
                            {
                                Value = "https://blogs2.msdn.net"
                            }
                        }
                    });
                    ctx.Add(new WebSite
                    {
                        Url = "https://www.microsoft.com",
                        HyperLinks = new List<Hyperlink>
                        {
                            new Hyperlink
                            {
                                Value = "https://www.microsoft.com"
                            },
                            new Hyperlink
                            {
                                Value = "https://www.visualstudio.com"
                            }
                        }
                    });
                    await ctx.SaveChangesAsync();
                }

                using (var repo = new TestBlogEFRepository())
                {
                    var sites = await repo.GetAsync(includes: w => w.HyperLinks).ToList();
                    sites.Should().HaveCount(2);
                    sites.Any(s => s.Url.Contains("msdn")).Should().BeTrue();
                    sites.Any(s => s.Url.Contains("microsoft")).Should().BeTrue();

                    var site = sites.FirstOrDefault(s => s.Url.Contains("msdn"));
                    site.HyperLinks.Should().HaveCount(2);
                    site.HyperLinks.Any(u => u.Value.Contains("blogs.")).Should().BeTrue();
                    site.HyperLinks.Any(u => u.Value.Contains("blogs2.")).Should().BeTrue();
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task EFRepository_GetByIdAsync_AsExpected()
        {
            try
            {
                Guid? id = null;
                using (var ctx = new TestDbContext())
                {
                    var w = new WebSite
                    {
                        Url = "https://blogs.msdn.net"
                    };
                    ctx.Add(w);
                    ctx.Add(new WebSite
                    {
                        Url = "https://www.microsoft.com"
                    });
                    await ctx.SaveChangesAsync();
                    id = w.Id;
                }

                using (var repo = new TestBlogEFRepository())
                {
                    var result = await repo.GetByIdAsync(id);
                    result.Should().NotBeNull();
                    result.Url.Should().Be("https://blogs.msdn.net");
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        #endregion

        #region DeletionTest

        [Fact]
        public async Task EFRepository_Physical_Deletion()
        {
            try
            {
                using (var ctx = new TestDbContext())
                {
                    var entity = new WebSite
                    {
                        Url = "http://dotnet.microsoft.com/"
                    };
                    ctx.Add(entity);
                    await ctx.SaveChangesAsync();
                }
                using (var repo = new TestBlogEFRepository())
                {
                    repo.Get().Count().Should().Be(1);
                    repo.Get(includeDeleted: true).Count().Should().Be(1);
                }
                using (var repo = new TestBlogEFRepository())
                {
                    var entity = repo.Get().FirstOrDefault();
                    entity.Should().NotBeNull();
                    repo.MarkForDelete(entity, true);
                    await repo.SaveAsync();
                }
                using (var repo = new TestBlogEFRepository())
                {
                    repo.Get(includeDeleted: true).Count().Should().Be(0);
                    repo.Get().Count().Should().Be(0);
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        [Fact]
        public async Task EFRepository_Logical_Deletion()
        {
            try
            {
                using (var ctx = new TestDbContext())
                {
                    var entity = new WebSite
                    {
                        Url = "http://dotnet.microsoft.com/"
                    };
                    ctx.Add(entity);
                    await ctx.SaveChangesAsync();
                }
                using (var repo = new TestBlogEFRepository())
                {
                    repo.Get().Count().Should().Be(1);
                    repo.Get(includeDeleted: true).Count().Should().Be(1);
                }
                using (var repo = new TestBlogEFRepository())
                {
                    var entity = repo.Get().FirstOrDefault();
                    entity.Should().NotBeNull();
                    repo.MarkForDelete(entity);
                    await repo.SaveAsync();
                }
                using (var repo = new TestBlogEFRepository())
                {
                    repo.Get(includeDeleted: true).Count().Should().Be(1);
                    var entity = repo.Get(includeDeleted: true).FirstOrDefault();
                    entity.DeletionDate.Should().BeSameDateAs(DateTime.Today);
                    repo.Get().Count().Should().Be(0);
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        #endregion

    }
}
