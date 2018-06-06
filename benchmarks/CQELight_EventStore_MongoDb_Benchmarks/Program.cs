using CQELight;
using CQELight.Abstractions.Events;
using CQELight.Bootstrapping.Notifications;
using CQELight.Dispatcher;
using CQELight.EventStore.MongoDb;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CQELight_EventStore_MongoDb_Benchmarks
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
            Console.WriteLine("Benchmark app for CQELight - Event Store - MongoDb - Preparation");

            var c = new ConfigurationBuilder()
                .AddJsonFile("config.json")
                .Build();
            new Bootstrapper()
                .UseMongoDbAsEventStore($"mongodb://{c["host"]}:{c["port"]}")
                .Bootstrapp();

            try
            {
                await EventStoreManager.Client.DropDatabaseAsync(Consts.CONST_DB_NAME).ConfigureAwait(false);
            }
            catch { }

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
