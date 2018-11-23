using BenchmarkDotNet.Attributes;
using CQELight.EventStore;
using CQELight.EventStore.CosmosDb;
using CQELight.EventStore.CosmosDb.Common;
using CQELight.EventStore.CosmosDb.Models;
using CQELight_Benchmarks.Models;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQELight.Tools.Extensions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Linq;

namespace CQELight_Benchmarks.Benchmarks
{
    public class CosmosDbEventStoreBenchmark : BaseBenchmark
    {
        #region BenchmarkDotNet
        
        [GlobalSetup]
        public void GlobalSetup()
        {
            CleanDatabases();
            AggregateId = Guid.NewGuid();
        }

        [IterationSetup(Targets = new[] { nameof(StoreRangeDomainEvent) })]
        public void IterationSetup()
        {
            CleanDatabases();
        }

        //[GlobalSetup(Targets = new[] { nameof(RehydrateAggregate) })]
        //public void GlobalSetup_Storage()
        //{
        //    CleanDatabases();
        //    StoreNDomainEvents();
        //}

        #endregion

        #region Private methods

        private void CleanDatabases()
        {
            EventStoreAzureDbContext.Activate(
                   new AzureDbConfiguration(GetConnectionInfos().URI, GetConnectionInfos().ConnectionString));

            var docs = EventStoreAzureDbContext.Client.CreateDocumentQuery<Event>(EventStoreAzureDbContext.EventsDatabaseLink).AsDocumentQuery();
            while (docs.HasMoreResults)
            {
                docs.ExecuteNextAsync<Document>().GetAwaiter().GetResult()
                    .DoForEach(e => EventStoreAzureDbContext.Client.DeleteDocumentAsync(documentLink: e.SelfLink).GetAwaiter().GetResult());
            }
        }

        //private void StoreNDomainEvents()
        //{
        //    EventStoreAzureDbContext.Activate(
        //        new AzureDbConfiguration(GetConnectionInfos().URI, GetConnectionInfos().ConnectionString));
        //    var store = new CosmosDbEventStore();
        //    for (int i = 0; i < N; i++)
        //    {
        //        store.StoreDomainEventAsync(new TestEvent(Guid.NewGuid(), AggregateId) { AggregateStringValue = "test", AggregateIntValue = N }).GetAwaiter().GetResult();
        //    }
        //}

        private static (string URI, string ConnectionString) GetConnectionInfos()
        {
            var cfg = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            return (cfg["CosmosDb_EventStore_Benchmarks:URI"], cfg["CosmosDb_EventStore_Benchmarks:PrimaryKey"]);
        }

        #endregion

        #region Public methods

        [Benchmark]
        public async Task StoreDomainEvent()
        {
            var store = new CosmosDbEventStore();
            await store.StoreDomainEventAsync(
                new TestEvent(Guid.NewGuid(), AggregateId)
                {
                    AggregateIntValue = 1,
                    AggregateStringValue = "test"
                });
        }

        [Benchmark]
        public async Task StoreRangeDomainEvent()
        {
            var store = new CosmosDbEventStore();
            for (int i = 0; i < N; i++)
            {
                await store.StoreDomainEventAsync(
                    new TestEvent(Guid.NewGuid(), AggregateId)
                    {
                        AggregateIntValue = 1,
                        AggregateStringValue = "test"
                    });
            }
        }

        [Benchmark]
        public async Task GetEventsByAggregateId()
        {
            var store = new CosmosDbEventStore();
            var evt = await store.GetEventsFromAggregateIdAsync<BenchmarkSimpleEvent>
            (
               AggregateId
            );
        }

        //[Benchmark]
        //public async Task RehydrateAggregate()
        //{
        //    var store = new CosmosDbEventStore();
        //    var agg = await store.GetRehydratedAggregateAsync<TestAggregate>(AggregateId);
        //}        

        #endregion

    }
}
