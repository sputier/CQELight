using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using MongoDB.Driver;
using System.Linq;
using CQELight.TestFramework;
using FluentAssertions;

namespace CQELight.DAL.MongoDb.Integration.Tests
{
    public class MongoRepositoryTests : BaseUnitTestClass
    {
        #region Ctor & members

        public MongoRepositoryTests()
        {
            if (MongoDbContext.MongoClient == null)
            {
                MongoDbContext.MongoClient
                    = new MongoClient(new MongoUrlBuilder
                    {
                        Server = new MongoServerAddress("localhost")
                    }.ToMongoUrl());
            }
        }

        private IMongoCollection<T> GetCollection<T>()
            => MongoDbContext.MongoClient
                    .GetDatabase("DefaultDatabase")
                    .GetCollection<T>(typeof(T).Name);

        private void DeleteAll()
        {
            GetCollection<Hyperlink>().DeleteMany(FilterDefinition<Hyperlink>.Empty);
            GetCollection<WebSite>().DeleteMany(FilterDefinition<WebSite>.Empty);
            GetCollection<Post>().DeleteMany(FilterDefinition<Post>.Empty);
            GetCollection<Comment>().DeleteMany(FilterDefinition<Comment>.Empty);
            GetCollection<User>().DeleteMany(FilterDefinition<User>.Empty);
        }

        #endregion

        #region Get

        [Fact]
        public async Task MongoRepository_SimpleGet_NoFilter_NoOrder_NoIncludes_Should_Returns_All()
        {
            try
            {
                var collection = GetCollection<WebSite>();
                await collection.InsertManyAsync(new[]
                {
                    new WebSite
                    {
                        Url = "https://blogs.msdn.net"
                    },
                    new WebSite
                    {
                        Url = "https://www.microsoft.com"
                    }
                });

                using (var repo = new MongoRepository<WebSite>())
                {
                    var sites = await repo.GetAsync().ToList().ConfigureAwait(false);
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
        public async Task MongoRepository_SimpleGet_WithFilter_NoOrder_NoIncludes_Should_Returns_FilteredItemsOnly()
        {
            try
            {
                var collection = GetCollection<WebSite>();
                await collection.InsertManyAsync(new[]
                {
                    new WebSite
                    {
                        Url = "https://blogs.msdn.net"
                    },
                    new WebSite
                    {
                        Url = "https://www.microsoft.com"
                    }
                });

                using (var repo = new MongoRepository<WebSite>())
                {
                    var sites = await repo.GetAsync(w => w.Url.Contains("msdn")).ToList().ConfigureAwait(false);
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
        public async Task MongoRepository_SimpleGet_NoFilter_NohOrder_NoIncludes_WithDeleted_Should_GetAll()
        {
            try
            {
                var collection = GetCollection<WebSite>();
                await collection.InsertManyAsync(new[]
                {
                    new WebSite
                    {
                        Url = "https://blogs.msdn.net"
                    },
                    new WebSite
                    {
                        Url = "https://www.microsoft.com",
                        Deleted = true,
                        DeletionDate = DateTime.Now
                    }
                });

                using (var repo = new MongoRepository<WebSite>())
                {
                    var sites = await repo.GetAsync(includeDeleted: true).ToList().ConfigureAwait(false);
                    sites.Should().HaveCount(2);
                    sites.Any(s => s.Url.Contains("msdn")).Should().BeTrue();
                    sites.Any(s => s.Url.Contains("microsoft")).Should().BeTrue();
                    sites.Any(s => s.Deleted).Should().BeTrue();
                    sites.Any(s => !s.Deleted).Should().BeTrue();

                    var undeletedSites = await repo.GetAsync().ToList().ConfigureAwait(false);
                    undeletedSites.Should().HaveCount(1);
                    undeletedSites.Any(s => s.Url.Contains("msdn")).Should().BeTrue();
                    undeletedSites.Any(s => s.Url.Contains("microsoft")).Should().BeFalse();
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        [Fact]
        public async Task MongoRepository_SimpleGet_NoFilter_WithOrder_NoIncludes_Should_Respect_Order()
        {
            try
            {
                var collection = GetCollection<WebSite>();
                await collection.InsertManyAsync(new[]
                {
                    new WebSite
                    {
                        Url = "https://www.microsoft.com"
                    },
                    new WebSite
                    {
                        Url = "https://blogs.msdn.net"
                    }
                });

                using (var repo = new MongoRepository<WebSite>())
                {
                    var sites = await repo.GetAsync(orderBy: b => b.Url).ToList().ConfigureAwait(false);
                    sites.Should().HaveCount(2);
                    sites.Any(s => s.Url.Contains("msdn")).Should().BeTrue();
                    sites.Any(s => s.Url.Contains("microsoft")).Should().BeTrue();
                    sites[0].Url.Should().Contain("msdn");
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        [Fact]
        public async Task MongoRepository_SimpleGet_NoFilter_NoOrder_WithIncludes_Should_GetLinkedData()
        {
            try
            {
                var collection = GetCollection<WebSite>();
                await collection.InsertManyAsync(new[]
                {
                   new WebSite
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
                    },
                   new WebSite
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
                    }
                });

                using (var repo = new MongoRepository<WebSite>())
                {
                    var sites = await repo.GetAsync(includes: w => w.HyperLinks).ToList().ConfigureAwait(false);
                    sites.Should().HaveCount(2);
                    sites.Any(s => s.Url.Contains("msdn")).Should().BeTrue();
                    sites.Any(s => s.Url.Contains("microsoft")).Should().BeTrue();

                    var site = sites.Find(s => s.Url.Contains("msdn"));
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
        public async Task MongoRepository_GetByIdAsync_Guid_AsExpected()
        {
            try
            {
                Guid id = Guid.NewGuid();
                var collection = GetCollection<WebSite>();
                var site1 = new WebSite
                {
                    Url = "https://blogs.msdn.net",
                };
                var site2 = new WebSite
                {
                    Url = "https://www.microsoft.com"
                };
                site1.FakePersistenceId(id);
                await collection.InsertManyAsync(new[] { site1, site2 });

                using (var repo = new MongoRepository<WebSite>())
                {
                    var result = await repo.GetByIdAsync(id).ConfigureAwait(false);
                    result.Should().NotBeNull();
                    result.Url.Should().Be("https://blogs.msdn.net");
                }
            }
            finally
            {
                DeleteAll();
            }
        }
        [Fact]
        public async Task MongoRepository_GetByIdAsync_CustomId_AsExpected()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task MongoRepository_GetByIdAsync_ComplexId_AsExpected()
        {
            throw new NotImplementedException();
        }

        #endregion

        //#region Insert

        //[Fact]
        //public async Task MongoRepository_Insert_AsExpected()
        //{
        //    try
        //    {
        //        using (var repo = new TestBlogMongoRepository())
        //        {
        //            var b = new WebSite
        //            {
        //                Url = "http://www.microsoft.com"
        //            };
        //            repo.MarkForInsert(b);
        //            await repo.SaveAsync().ConfigureAwait(false);
        //        }

        //        using (var repo = new TestBlogMongoRepository())
        //        {
        //            var testB = await repo.GetAsync().ToList().ConfigureAwait(false);
        //            testB.Should().HaveCount(1);
        //            testB[0].Url.Should().Be("http://www.microsoft.com");
        //        }
        //    }
        //    finally
        //    {
        //        DeleteAll();
        //    }
        //}

        //[Fact]
        //public async Task MongoRepository_Insert_Id_AlreadySet_AsExpected()
        //{
        //    try
        //    {
        //        using (var repo = new TestBlogMongoRepository())
        //        {
        //            var b = new WebSite
        //            {
        //                Url = "http://www.microsoft.com"
        //            };
        //            typeof(WebSite).GetProperty("Id").SetValue(b, Guid.NewGuid());
        //            repo.MarkForInsert(b);
        //            await repo.SaveAsync().ConfigureAwait(false);
        //        }

        //        using (var repo = new TestBlogMongoRepository())
        //        {
        //            var testB = await repo.GetAsync().ToList().ConfigureAwait(false);
        //            testB.Should().HaveCount(1);
        //            testB[0].Url.Should().Be("http://www.microsoft.com");
        //        }
        //    }
        //    finally
        //    {
        //        DeleteAll();
        //    }
        //}

        //#endregion

        //#region Update

        //[Fact]
        //public async Task MongoRepository_Update_AsExpected()
        //{
        //    try
        //    {
        //        using (var ctx = new TestDbContext())
        //        {
        //            var b = new WebSite
        //            {
        //                Url = "http://www.microsoft.com"
        //            };
        //            ctx.Add(b);
        //            await ctx.SaveChangesAsync().ConfigureAwait(false);
        //        }
        //        using (var repo = new TestBlogMongoRepository())
        //        {
        //            var w = await repo.GetAsync().FirstOrDefault().ConfigureAwait(false);
        //            w.Url = "https://www.microsoft.com";
        //            repo.MarkForUpdate(w);
        //            await repo.SaveAsync().ConfigureAwait(false);
        //        }

        //        using (var repo = new TestBlogMongoRepository())
        //        {
        //            var testB = await repo.GetAsync().ToList().ConfigureAwait(false);
        //            testB.Should().HaveCount(1);
        //            testB[0].Url.Should().Be("https://www.microsoft.com");
        //        }
        //    }
        //    finally
        //    {
        //        DeleteAll();
        //    }
        //}

        ////[Fact]
        ////public async Task MongoRepository_Update_NotTracked_AsExpected()
        ////{
        ////    try
        ////    {
        ////        using (var ctx = new TestDbContext())
        ////        {
        ////            var b = new WebSite
        ////            {
        ////                Url = "http://www.microsoft.com"
        ////            };
        ////            ctx.Add(b);
        ////            await ctx.SaveChangesAsync().ConfigureAwait(false);
        ////        }
        ////        using (var repo = new TestBlogMongoRepository())
        ////        {
        ////            var w = await repo.GetAsync(tracked: false).FirstOrDefault().ConfigureAwait(false);
        ////            w.Url = "https://www.microsoft.com";
        ////            repo.MarkForUpdate(w);
        ////            await repo.SaveAsync().ConfigureAwait(false);
        ////        }

        ////        using (var repo = new TestBlogMongoRepository())
        ////        {
        ////            var testB = await repo.GetAsync().ToList().ConfigureAwait(false);
        ////            testB.Should().HaveCount(1);
        ////            testB[0].Url.Should().Be("https://www.microsoft.com");
        ////        }
        ////    }
        ////    finally
        ////    {
        ////        DeleteAll();
        ////    }
        ////}

        //[Fact]
        //public async Task MongoRepository_Update_NotExisting_InBDD_AsExpected()
        //{
        //    try
        //    {
        //        using (var repo = new TestBlogMongoRepository())
        //        {
        //            var b = new WebSite
        //            {
        //                Url = "http://www.microsoft.com"
        //            };
        //            b.FakePersistenceId(Guid.NewGuid());
        //            repo.MarkForUpdate(b);
        //            await repo.SaveAsync().ConfigureAwait(false);
        //        }

        //        using (var repo = new TestBlogMongoRepository())
        //        {
        //            var testB = await repo.GetAsync().ToList().ConfigureAwait(false);
        //            testB.Should().HaveCount(1);
        //            testB[0].Url.Should().Be("http://www.microsoft.com");
        //        }
        //    }
        //    finally
        //    {
        //        DeleteAll();
        //    }
        //}

        //#endregion

        //#region DeletionTest

        //[Fact]
        //public void MongoRepository_MarkIdForDelete_IdNotFound()
        //{
        //    try
        //    {
        //        using (var repo = new TestBlogMongoRepository())
        //        {
        //            Assert.Throws<InvalidOperationException>(() => repo.MarkIdForDelete(Guid.NewGuid()));
        //        }
        //    }
        //    finally
        //    {
        //        DeleteAll();
        //    }
        //}

        //[Fact]
        //public async Task MongoRepository_Physical_Deletion_ById()
        //{
        //    try
        //    {
        //        Guid? id = null;
        //        using (var ctx = new TestDbContext())
        //        {
        //            var entity = new WebSite
        //            {
        //                Url = "http://dotnet.microsoft.com/"
        //            };
        //            ctx.Add(entity);
        //            await ctx.SaveChangesAsync().ConfigureAwait(false);
        //            id = entity.Id;
        //        }
        //        using (var repo = new TestBlogMongoRepository())
        //        {
        //            repo.Get().Count().Should().Be(1);
        //            repo.Get(includeDeleted: true).Count().Should().Be(1);
        //        }
        //        using (var repo = new TestBlogMongoRepository())
        //        {
        //            var entity = repo.Get().FirstOrDefault();
        //            entity.Should().NotBeNull();
        //            repo.MarkIdForDelete(id, true);
        //            await repo.SaveAsync().ConfigureAwait(false);
        //        }
        //        using (var repo = new TestBlogMongoRepository())
        //        {
        //            repo.Get(includeDeleted: true).Count().Should().Be(0);
        //            repo.Get().Count().Should().Be(0);
        //        }
        //    }
        //    finally
        //    {
        //        DeleteAll();
        //    }
        //}

        //[Fact]
        //public async Task MongoRepository_Logical_Deletion_ById()
        //{
        //    try
        //    {
        //        Guid? id = null;
        //        using (var ctx = new TestDbContext())
        //        {
        //            var entity = new WebSite
        //            {
        //                Url = "http://dotnet.microsoft.com/"
        //            };
        //            ctx.Add(entity);
        //            await ctx.SaveChangesAsync().ConfigureAwait(false);
        //            id = entity.Id;
        //        }
        //        using (var repo = new TestBlogMongoRepository())
        //        {
        //            repo.Get().Count().Should().Be(1);
        //            repo.Get(includeDeleted: true).Count().Should().Be(1);
        //        }
        //        using (var repo = new TestBlogMongoRepository())
        //        {
        //            var entity = repo.Get().FirstOrDefault();
        //            entity.Should().NotBeNull();
        //            repo.MarkIdForDelete(id);
        //            await repo.SaveAsync().ConfigureAwait(false);
        //        }
        //        using (var repo = new TestBlogMongoRepository())
        //        {
        //            repo.Get(includeDeleted: true).Count().Should().Be(1);
        //            var entity = repo.Get(includeDeleted: true).FirstOrDefault();
        //            entity.DeletionDate.Should().BeSameDateAs(DateTime.Today);
        //            repo.Get().Count().Should().Be(0);
        //        }
        //    }
        //    finally
        //    {
        //        DeleteAll();
        //    }
        //}

        //[Fact]
        //public async Task MongoRepository_Physical_Deletion()
        //{
        //    try
        //    {
        //        using (var ctx = new TestDbContext())
        //        {
        //            var entity = new WebSite
        //            {
        //                Url = "http://dotnet.microsoft.com/"
        //            };
        //            ctx.Add(entity);
        //            await ctx.SaveChangesAsync().ConfigureAwait(false);
        //        }
        //        using (var repo = new TestBlogMongoRepository())
        //        {
        //            repo.Get().Count().Should().Be(1);
        //            repo.Get(includeDeleted: true).Count().Should().Be(1);
        //        }
        //        using (var repo = new TestBlogMongoRepository())
        //        {
        //            var entity = repo.Get().FirstOrDefault();
        //            entity.Should().NotBeNull();
        //            repo.MarkForDelete(entity, true);
        //            await repo.SaveAsync().ConfigureAwait(false);
        //        }
        //        using (var repo = new TestBlogMongoRepository())
        //        {
        //            repo.Get(includeDeleted: true).Count().Should().Be(0);
        //            repo.Get().Count().Should().Be(0);
        //        }
        //    }
        //    finally
        //    {
        //        DeleteAll();
        //    }
        //}

        //[Fact]
        //public async Task MongoRepository_Logical_Deletion()
        //{
        //    try
        //    {
        //        using (var ctx = new TestDbContext())
        //        {
        //            var entity = new WebSite
        //            {
        //                Url = "http://dotnet.microsoft.com/"
        //            };
        //            ctx.Add(entity);
        //            await ctx.SaveChangesAsync().ConfigureAwait(false);
        //        }
        //        using (var repo = new TestBlogMongoRepository())
        //        {
        //            repo.Get().Count().Should().Be(1);
        //            repo.Get(includeDeleted: true).Count().Should().Be(1);
        //        }
        //        using (var repo = new TestBlogMongoRepository())
        //        {
        //            var entity = repo.Get().FirstOrDefault();
        //            entity.Should().NotBeNull();
        //            repo.MarkForDelete(entity);
        //            await repo.SaveAsync().ConfigureAwait(false);
        //        }
        //        using (var repo = new TestBlogMongoRepository())
        //        {
        //            repo.Get(includeDeleted: true).Count().Should().Be(1);
        //            var entity = repo.Get(includeDeleted: true).FirstOrDefault();
        //            entity.DeletionDate.Should().BeSameDateAs(DateTime.Today);
        //            repo.Get().Count().Should().Be(0);
        //        }
        //    }
        //    finally
        //    {
        //        DeleteAll();
        //    }
        //}

        //#endregion

        //#region NotNaviguable

        //[Fact]
        //public async Task MongoRepository_Update_Blog_Should_Update_Posts_But_Not_User()
        //{
        //    var site = new WebSite();
        //    site.Url = "http://msdn.dotnet.com";
        //    site.Posts = new List<Post>();
        //    site.Posts.Add(new Post
        //    {
        //        Content = "test content",
        //        WebSite = site,
        //        QuickUrl = "http://msdn.dotnet.com/test",
        //    });
        //    var user = new User
        //    {
        //        Name = "toto",
        //        LastName = "titi"
        //    };
        //    user.Posts = new List<Post>();
        //    user.Posts.Add(site.Posts.First());
        //    user.Comments = new List<Comment>();
        //    user.Comments.Add(new Comment("test comment", user, site.Posts.First()));

        //    site.Posts.First().Writer = user;

        //    using (var repo = new TestBlogMongoRepository())
        //    {
        //        repo.MarkForInsert(site);
        //        await repo.SaveAsync().ConfigureAwait(false);
        //    }

        //    WebSite dbSite = null;
        //    using (var repo = new TestBlogMongoRepository())
        //    {
        //        dbSite = repo.GetWithPostAndComment();
        //    }
        //    dbSite.Should().NotBeNull();

        //    var post = dbSite.Posts.First();

        //    post.QuickUrl += "2";
        //    post.Writer.LastName += "2";
        //    post.Writer.Name += "2";
        //    post.Writer.Comments.First().Value += "2";

        //    using (var repo = new TestBlogMongoRepository())
        //    {
        //        repo.MarkForUpdate(dbSite);
        //        await repo.SaveAsync().ConfigureAwait(false);
        //    }

        //    using (var repo = new TestBlogMongoRepository())
        //    {
        //        dbSite = repo.GetWithPostAndComment();
        //        dbSite.Should().NotBeNull();
        //        post = dbSite.Posts.First();

        //        post.QuickUrl.Should().Be("http://msdn.dotnet.com/test2");
        //        post.Writer.Comments.First().Value.Should().Be("test comment2");
        //        post.Writer.LastName.Should().Be("titi");
        //        post.Writer.Name.Should().Be("toto");
        //    }
        //}

        //#endregion

        //#region DeleteCascade

        //[Fact]
        //public async Task MongoRepository_MarkForDelete_DeleteBlog_Should_Delete_Posts_Logical_Cascade()
        //{
        //    using (var ctx = new TestDbContext())
        //    {
        //        var entity = new WebSite
        //        {
        //            Url = "http://dotnet.microsoft.com/"
        //        };
        //        var post = new Post
        //        {
        //            WebSite = entity,
        //            Published = true,
        //            PublicationDate = DateTime.Today,
        //            QuickUrl = "http://dotnet.microsoft.com/abgfazJSQg2",
        //            Version = 1,
        //            Content = "test data"
        //        };
        //        entity.Posts = new List<Post> { post };
        //        ctx.Add(entity);
        //        ctx.SaveChanges();
        //    }

        //    using (var ctx = new TestDbContext())
        //    {
        //        ctx.Set<WebSite>().Should().HaveCount(1);
        //        ctx.Set<Post>().Should().HaveCount(1);
        //    }

        //    using (var rep = new TestBlogMongoRepository())
        //    {
        //        var b = await rep.GetAsync().First().ConfigureAwait(false);
        //        rep.MarkForDelete(b);
        //        await rep.SaveAsync().ConfigureAwait(false);
        //    }

        //    using (var ctx = new TestDbContext())
        //    {
        //        ctx.Set<WebSite>().Should().HaveCount(1);
        //        ctx.Set<Post>().Should().HaveCount(1);

        //        ctx.Set<WebSite>().Where(e => !e.Deleted).Should().HaveCount(0);
        //        ctx.Set<Post>().Where(e => !e.Deleted).Should().HaveCount(1); // Le soft delete est à gérer niveau repository
        //    }
        //}

        //[Fact]
        //public async Task MongoRepository_MarkForDelete_DeleteBlog_Should_Delete_Posts_Physical_Cascade()
        //{
        //    using (var ctx = new TestDbContext())
        //    {
        //        var entity = new WebSite
        //        {
        //            Url = "http://dotnet.microsoft.com/"
        //        };
        //        var post = new Post
        //        {
        //            WebSite = entity,
        //            Published = true,
        //            PublicationDate = DateTime.Today,
        //            QuickUrl = "http://dotnet.microsoft.com/abgfazJSQg2",
        //            Version = 1,
        //            Content = "test data"
        //        };
        //        entity.Posts = new List<Post> { post };
        //        ctx.Add(entity);
        //        ctx.SaveChanges();
        //    }

        //    using (var ctx = new TestDbContext())
        //    {
        //        ctx.Set<WebSite>().Should().HaveCount(1);
        //        ctx.Set<Post>().Should().HaveCount(1);
        //    }

        //    using (var rep = new TestBlogMongoRepository())
        //    {
        //        var b = await rep.GetAsync().First().ConfigureAwait(false);
        //        rep.MarkForDelete(b, true);
        //        await rep.SaveAsync().ConfigureAwait(false);
        //    }

        //    using (var ctx = new TestDbContext())
        //    {
        //        ctx.Set<WebSite>().Should().HaveCount(0);
        //        ctx.Set<Post>().Should().HaveCount(0);
        //    }
        //}

        //[Fact]
        //public async Task MongoRepository_MarkForDelete_Logical_Deletion_Globally_Deactivated()
        //{
        //    EFCoreInternalExecutionContext.DisableLogicalDeletion = true;
        //    using (var ctx = new TestDbContext())
        //    {
        //        var entity = new WebSite
        //        {
        //            Url = "http://dotnet.microsoft.com/"
        //        };
        //        var post = new Post
        //        {
        //            WebSite = entity,
        //            Published = true,
        //            PublicationDate = DateTime.Today,
        //            QuickUrl = "http://dotnet.microsoft.com/abgfazJSQg2",
        //            Version = 1,
        //            Content = "test data"
        //        };
        //        entity.Posts = new List<Post> { post };
        //        ctx.Add(entity);
        //        ctx.SaveChanges();
        //    }

        //    using (var ctx = new TestDbContext())
        //    {
        //        ctx.Set<WebSite>().Should().HaveCount(1);
        //        ctx.Set<Post>().Should().HaveCount(1);
        //    }

        //    using (var rep = new TestBlogMongoRepository())
        //    {
        //        var b = await rep.GetAsync().First().ConfigureAwait(false);
        //        rep.MarkForDelete(b);
        //        await rep.SaveAsync().ConfigureAwait(false);
        //    }

        //    using (var ctx = new TestDbContext())
        //    {
        //        ctx.Set<WebSite>().Should().HaveCount(0);
        //        ctx.Set<Post>().Should().HaveCount(0);
        //    }
        //}

        //#endregion
    }
}
