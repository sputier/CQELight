using CQELight;
using CQELight.Abstractions.Events;
using System;
using System.Threading.Tasks;
using CQELight.EventStore.CosmosDb;
using CQELight.Bootstrapping.Notifications;
using System.Collections.Generic;
using CQELight.Dispatcher;
using Microsoft.Azure.Documents.Client;
using CQELight.EventStore.CosmosDb.Common;

namespace CQELight_EventStore_CosmosDb_Benchmarks
{

    class BenchmarkEvent : BaseDomainEvent
    {
        public BenchmarkEvent(Guid id)
        {
            Id = id;
        }

        public int Loop { get; set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Benchmark app for CQELight - Event Store - CosmosDb - Preparation");

            try
            {
                await new DocumentClient(new Uri("https://localhost:8081"), "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==")
                    .DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(EventStoreAzureDbContext.CONST_DB_NAME)).ConfigureAwait(false);
            }
            catch { }

            new Bootstrapper()
                .UseCosmosDbAsEventStore("https://localhost:8081", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==")
                .Bootstrapp(out List<BootstrapperNotification> notifs);


            Console.WriteLine("Press any key to begin");

            Console.ReadKey();

            Console.WriteLine("-- BENCHMARK -- Begin 100 loops");
            await Loop(100).ConfigureAwait(false);

            Console.WriteLine("-- BENCHMARK -- Begin 1000 loops");
            await Loop(1000).ConfigureAwait(false);

            Console.WriteLine("-- BENCHMARK -- Begin 10000 loops");
            await Loop(10000).ConfigureAwait(false);

            Console.WriteLine("-- BENCHMARK -- Begin 100000 loops");
            await Loop(100000).ConfigureAwait(false);

            Console.WriteLine("-- BENCHMARK -- Begin 1000000 loops");
            await Loop(1000000).ConfigureAwait(false);

            Console.WriteLine("Press any key to exit");

            Console.ReadKey();
        }

        static async Task Loop(int loops)
        {
            DateTime startDate = DateTime.Now;
            var tasks = new List<Task>();
            for (int i = 0; i < loops; i++)
            {
                var eventToCreate = new BenchmarkEvent(Guid.NewGuid()) { Loop = i };
                tasks.Add(CoreDispatcher.PublishEventAsync(eventToCreate));
            }
            await Task.WhenAll(tasks);
            DateTime endDate = DateTime.Now;
            Console.WriteLine($"For {loops} iterations, took {(endDate - startDate).TotalMilliseconds} ms");
        }
    }
}
