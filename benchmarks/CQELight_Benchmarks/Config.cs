using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Parameters;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using CQELight_Benchmarks.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CQELight_Benchmarks
{
    internal class Config : ManualConfig
    {

        public Config()
        {
#if DEBUG
            Add(JitOptimizationsValidator.DontFailOnError); // ALLOW NON-OPTIMIZED DLLS        
            Add(DefaultConfig.Instance.GetLoggers().ToArray()); 
            Add(DefaultConfig.Instance.GetExporters().ToArray()); 
            Add(DefaultConfig.Instance.GetColumnProviders().ToArray()); 
#endif
            Set(new CustomOrderProvider());
        }

        private class CustomOrderProvider : IOrderProvider
        {
            public bool SeparateLogicalGroups => false;

            public IEnumerable<Benchmark> GetExecutionOrder(Benchmark[] benchmarks)
            {
                return
                    from benchmark in benchmarks
                    orderby benchmark.Target.Method.GetCustomAttribute<BenchmarkOrderAttribute>()?.Order ?? 1
                    select benchmark;
            }

            public IEnumerable<Benchmark> GetSummaryOrder(Benchmark[] benchmarks, Summary summary) =>
                from benchmark in benchmarks
                orderby summary[benchmark].ResultStatistics.Mean
                select benchmark;

            public string GetGroupKey(Benchmark benchmark, Summary summary) => null;

            public string GetHighlightGroupKey(Benchmark benchmark)
            {
                return benchmark.Parameters.DisplayInfo;
            }

            public string GetLogicalGroupKey(IConfig config, Benchmark[] allBenchmarks,
                Benchmark benchmark)
                => "*";

            public IEnumerable<IGrouping<string, Benchmark>> GetLogicalGroupOrder
                (IEnumerable<IGrouping<string, Benchmark>> logicalGroups)
                => logicalGroups;
        }

    }


}
