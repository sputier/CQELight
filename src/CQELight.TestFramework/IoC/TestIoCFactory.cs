using CQELight.Abstractions.IoC.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.TestFramework.IoC
{
    class TestIoCFactory : IScopeFactory
    {

        #region Properties

        public ConcurrentDictionary<Type, object> Instances { get; private set; }
            = new ConcurrentDictionary<Type, object>();

        #endregion

        #region IScopeFactory methods

        public IScope CreateScope()
            => new TestIoCScope(Instances.ToDictionary(k => k.Key, v => v.Value));

        #endregion

    }
}
