using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight_Benchmarks
{
    public class NetCoreConfig : ManualConfig
    {

        public NetCoreConfig()
        {
            Add(Job.Default.With(
                CsProjCoreToolchain.NetCoreApp21));
        }

    }
}
