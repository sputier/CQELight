using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using CQELight;
using CQELight.Tools.Extensions;
using CQELight_Benchmarks.Benchmarks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CQELight_Benchmarks
{
    public enum TestArea
    {
        EventStore
    }

    internal static class Consts
    {
        public const string CONST_EVT_IDS_DIR = @"C:\temp_dev\evt_ids\";
        public const string CONST_AGG_IDS_DIR = @"C:\temp_dev\agg_ids\";
    }

    internal static class Program
    {

        #region Public properties

        public static IConfiguration GlobalConfiguration { get; private set; }
        public static List<Guid> AggregateIds { get; private set; }

        #endregion

        #region Main

        static void Main(string[] args)
        {
            GlobalConfiguration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            AggregateIds = new List<Guid>();

            Console.WriteLine("CQELight Benchmark application");
            Console.WriteLine("---- MENU -----");

            if (Directory.Exists(Consts.CONST_EVT_IDS_DIR))
            {
                Directory.Delete(Consts.CONST_EVT_IDS_DIR, true);
            }
            Directory.CreateDirectory(Consts.CONST_EVT_IDS_DIR);
            if (Directory.Exists(Consts.CONST_AGG_IDS_DIR))
            {
                Directory.Delete(Consts.CONST_AGG_IDS_DIR, true);
            }
            Directory.CreateDirectory(Consts.CONST_AGG_IDS_DIR);

            var testArea = GetTestArea();

            ExecuteTest(testArea);

            Console.WriteLine("Benchmark finished, press Enter to exit");
            Console.ReadLine();

        }

        #endregion

        #region Private methods

        private static void ExecuteTest(TestArea testArea)
        {
            Summary summary = null;
            while (summary == null)
            {
                if (testArea == TestArea.EventStore)
                {
                    Console.WriteLine("Please select Event Store provider you want to test");
                    Console.WriteLine("\t1. MongoDb");
                    Console.WriteLine("\t2. CosmosDb");

                    var result = Console.ReadKey();
                    Console.WriteLine();

                    switch (result.Key)
                    {
                        case ConsoleKey.NumPad1:
                        case ConsoleKey.D1:
                            new Bootstrapper()
                                .UseMongoDbAsEventStore("mongodb://" + GlobalConfiguration["MongoDb_EventStore_Benchmarks:Server"])
                                .Bootstrapp();
                            summary = BenchmarkRunner.Run<EventStoreBaseBenchmark>(new Config());
                            break;
                        case ConsoleKey.NumPad2:
                        case ConsoleKey.D2:
                            new Bootstrapper()
                                .UseCosmosDbAsEventStore(
                                    "https://localhost:8081",
                                    "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==")
                                .Bootstrapp();
                            summary = BenchmarkRunner.Run<EventStoreBaseBenchmark>(new Config());
                            break;
                    }

                }
            }
            Console.WriteLine(summary);
        }

        private static TestArea GetTestArea()
        {
            TestArea? testArea = null;
            while (!testArea.HasValue)
            {
                Console.WriteLine("Please select area you wish to benchmark");

                Console.WriteLine("\t1. Event store");
                var result = Console.ReadKey();
                Console.WriteLine();

                switch (result.Key)
                {
                    case ConsoleKey.NumPad1:
                    case ConsoleKey.D1:
                        testArea = TestArea.EventStore;
                        break;
                }
            }
            Console.WriteLine();
            return testArea.GetValueOrDefault();
        }

        #endregion

    }
}
