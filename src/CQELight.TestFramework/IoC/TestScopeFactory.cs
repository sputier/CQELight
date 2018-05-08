using CQELight.Abstractions.IoC.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.TestFramework.IoC
{
    /// <summary>
    /// A fully in memory test scope factory
    /// </summary>
    public class TestScopeFactory : IScopeFactory
    {
        #region Properties

        public Dictionary<Type, object> Instances { get; private set; }
            = new Dictionary<Type, object>();

        #endregion

        #region IScopeFactory methods

        public IScope CreateScope()
            => new TestScope(Instances.ToDictionary(k => k.Key, v => v.Value));

        #endregion

    }
}
