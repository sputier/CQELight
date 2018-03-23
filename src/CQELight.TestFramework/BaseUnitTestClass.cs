using CQELight.Dispatcher;
using CQELight.IoC;
using CQELight.TestFramework.IoC;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace CQELight.TestFramework
{
    /// <summary>
    /// Base class for unit testing classes.
    /// </summary>
    public class BaseUnitTestClass
    {

        #region Members

        private readonly TestIoCFactory _testFactory;

        #endregion

        #region Ctor

        protected BaseUnitTestClass()
        {
            UnitTestTools.IsInUnitTestMode = true;
            UnitTestTools.IsInIntegrationTestMode = GetType().Assembly.GetName().Name.Contains(".Integration.");

            if (!UnitTestTools.IsInIntegrationTestMode)
            {
                _testFactory = new TestIoCFactory();
                DIManager.Init(_testFactory);
                var loggerFactory = new LoggerFactory();
                loggerFactory.AddDebug();
                AddRegistrationFor<ILoggerFactory>(loggerFactory);
            }
        }

        #endregion

        #region Protected methods

        protected void CleanRegistrationInDispatcher()
        {
            if(UnitTestTools.IsInIntegrationTestMode)
            {
                CoreDispatcher.CleanRegistrations();
            }
        }

        protected void AddRegistrationFor<T>(object instance)
        {
            if (UnitTestTools.IsInIntegrationTestMode)
            {
                throw new InvalidOperationException("BaseUnitTestClass.AddRegistrationFor() : Cannot add registration into your IoC container. " +
                    "You have to manage it in your test initialization.");
            }
            _testFactory.Instances.AddOrUpdate(typeof(T), instance, (type, data) => instance);
        }

        #endregion

    }
}
