using CQELight.Abstractions.CQS.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight_Benchmarks.Models
{
    public class TestCommand : ICommand
    {
        public TestCommand(int i, bool simulateWork, int jobDuration)
        {
            I = i;
            SimulateWork = simulateWork;
            JobDuration = jobDuration;
        }

        public int I { get; }
        public bool SimulateWork { get; }
        public int JobDuration { get; }
    }
}
