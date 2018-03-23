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

    class TestBlogEFRepository : EFRepository<Blog>
    {
        public TestBlogEFRepository() : base(new TestDbContext())
        {
        }

        public Blog GetWithPostAndComment()
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
        }

        #endregion

        #region DeletionTest

        [Fact]
        public async Task EFRepository_Physical_Deletion()
        {
            using (var repo = new TestBlogEFRepository())
            {
                var entity = new Blog
                {
                    Url = "http://dotnet.microsoft.com/"
                };
                repo.MarkForInsert(entity);
                await repo.SaveAsync();
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

        [Fact]
        public async Task EFRepository_Logical_Deletion()
        {
            using (var repo = new TestBlogEFRepository())
            {
                var entity = new Blog
                {
                    Url = "http://dotnet.microsoft.com/"
                };
                repo.MarkForInsert(entity);
                await repo.SaveAsync();
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

        #endregion

    }
}
