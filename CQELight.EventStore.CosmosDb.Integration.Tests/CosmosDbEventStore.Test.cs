using System;
using System.Collections.Generic;
using System.Text;
using CQELight.TestFramework;


namespace CQELight.EventStore.CosmosDb.Integration.Tests
{
    public class CosmosDbEventStoreTest : BaseUnitTestClass
    {
		public CosmosDbEventStoreTest()
        {
            var cosmosDbEventStore = new CosmosDbEventStore();
        }
    }
}
