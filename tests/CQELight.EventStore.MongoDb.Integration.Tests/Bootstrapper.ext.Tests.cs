using CQELight.TestFramework;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CQELight.EventStore.MongoDb.Integration.Tests
{
    public class BootstrapperExtTests : BaseUnitTestClass
    {
        #region Ctor & members

        #endregion

        #region UseMongoDbAsEventStore

        [Fact]
        public void BootstrapperExt_UseMongoDbAsEventStore_ParamsTests()
        {
            Assert.Throws<ArgumentNullException>(() => new Bootstrapper().UseMongoDbAsEventStore(null));
            Assert.Throws<ArgumentException>(() => new Bootstrapper().UseMongoDbAsEventStore(new string[] { }));
            Assert.Throws<ArgumentException>(() => new Bootstrapper().UseMongoDbAsEventStore(new string[] { "__BADURL" }));
        }

        #endregion

    }
}
