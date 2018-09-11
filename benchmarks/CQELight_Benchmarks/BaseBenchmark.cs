﻿using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight_Benchmarks
{
    public abstract class BaseBenchmark
    {

        [Params(100, 1000, 10_000)]
        public int N;

        protected Random _random = new Random();


    }
}
