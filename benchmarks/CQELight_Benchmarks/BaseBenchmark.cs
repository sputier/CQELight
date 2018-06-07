using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight_Benchmarks
{
    [Config(typeof(NetCoreConfig))]
    public abstract class BaseBenchmark
    {

        [Params(100, 1_000, 10_000, 100_000, 1_000_000)]
        public int N;
        
    }
}
