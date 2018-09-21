using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight_Benchmarks
{
    public abstract class BaseBenchmark
    {

        public Guid AggregateId { get; protected set; }

        [Params(1000)]
        public int N;

        protected Random _random = new Random();


    }
}
