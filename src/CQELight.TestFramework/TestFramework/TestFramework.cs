using CQELight.Abstractions.Dispatcher.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.IoC;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQELight.TestFramework
{
    /// <summary>
    /// Test framework base entry point.
    /// </summary>
    public static class Test
    {
        #region Public static methods

        /// <summary>
        /// Create a synchronous action assertion.
        /// </summary>
        /// <param name="act">Action to invoke.</param>
        /// <returns>Assertion object to perform tests.</returns>
        public static TestFrameworkActionAssertion When(Action act)
            => new TestFrameworkActionAssertion(act);

        /// <summary>
        /// Create a synchronous action assertion.
        /// </summary>
        /// <param name="act">Action to invoke.</param>
        /// <param name="dispatcherMock">Dispatcher mock to test against.</param>
        /// <returns>Assertion object to perform tests.</returns>
        public static TestFrameworkActionAssertion When(Action act, Mock<IDispatcher> dispatcherMock)
            => new TestFrameworkActionAssertion(act, dispatcherMock);

        /// <summary>
        /// Create a asynchronous action assertion.
        /// </summary>
        /// <param name="act">Action to invoke.</param>
        /// <returns>Assertion object to perform tests.</returns>
        public static TestFrameworkAsyncActionAssertion WhenAsync(Func<Task> act)
            => new TestFrameworkAsyncActionAssertion(act);

        /// <summary>
        /// Create a asynchronous action assertion.
        /// </summary>
        /// <param name="act">Action to invoke.</param>
        /// <param name="dispatcherMock">Dispatcher mock to test against.</param>
        /// <returns>Assertion object to perform tests.</returns>
        public static TestFrameworkAsyncActionAssertion WhenAsync(Func<Task> act, Mock<IDispatcher> dispatcherMock)
            => new TestFrameworkAsyncActionAssertion(act, dispatcherMock);

        #endregion

    }
}
