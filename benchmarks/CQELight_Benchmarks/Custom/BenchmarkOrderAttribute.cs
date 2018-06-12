using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight_Benchmarks.Custom
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class BenchmarkOrderAttribute : Attribute
    {

        #region Properties

        public int Order { get; set; }

        #endregion

        #region Ctor

        public BenchmarkOrderAttribute(int order)
        {
            Order = order;
        }

        #endregion

    }
}
