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
        }

        #endregion

        #region Options ctor

        [Fact]
        public void BootstrapperOptions_Ctor_ParamsTests()
        {
            Assert.Throws<ArgumentNullException>(() => new MongoDbEventStoreBootstrapperConfiguration(null));
            Assert.Throws<ArgumentException>(() => new MongoDbEventStoreBootstrapperConfiguration(new string[] { }));
            Assert.Throws<ArgumentException>(() => new MongoDbEventStoreBootstrapperConfiguration(new string[] { "__BADURL" }));
        }

        #endregion

    }
}
