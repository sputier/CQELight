using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight_Benchmarks
{
    public abstract class BaseBenchmark
    {

        [Params(1000)]
        public int N;

        public Guid AggregateId = Guid.NewGuid();

        protected Random _random = new Random();


    }
}
