using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using CQELight;
using CQELight.Tools.Extensions;
using CQELight_Benchmarks.Benchmarks;
using System;
using System.Threading.Tasks;

namespace CQELight_Benchmarks
{
    public enum TestArea
    {
        EventStore
    }

    internal class Program
    {

        #region Main

        static void Main(string[] args)
        {
            Console.WriteLine("CQELight Benchmark application");
            Console.WriteLine("---- MENU -----");

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
                    Console.WriteLine();

                    var result = Console.ReadKey();

                    switch (result.Key)
                    {
                        case ConsoleKey.NumPad1:
                        case ConsoleKey.D1:
                            new Bootstrapper()
                                .UseMongoDbAsEventStore("mongodb://127.0.0.1")
                                .Bootstrapp();
                            summary = BenchmarkRunner.Run<MongoDbBenchmark>();
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
                Console.WriteLine();
                var result = Console.ReadKey();

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
