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

        private readonly IScope _providedScope;

        #endregion

        #region Ctor

        public TestScopeFactory()
        {

        }

        public TestScopeFactory(IScope providedScope)
        {
            _providedScope = providedScope;
        }

        #endregion

        #region IScopeFactory methods

        public IScope CreateScope()
            => _providedScope ?? new TestScope(Instances.ToDictionary(k => k.Key, v => v.Value));

        #endregion

    }
}
