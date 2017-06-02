using CQELight.Abstractions.Events.Interfaces;
using System.Threading;

namespace CQELight.Dispatcher
{
    /// <summary>
    /// Awaiter of event after command was send into buses.
    /// </summary>
    public class DispatcherAwaiter
    {
        
        #region Public methods

        /// <summary>
        /// Wait for a specific event type until timeout or event sent.
        /// </summary>
        /// <typeparam name="T">Type of event to wait for.</typeparam>
        /// <param name="timeout">Maximum timeout.</param>
        /// <returns>Instance of event if any, null otherwise.</returns>
        public T WaitForEvent<T>(ulong timeout = 1000) where T : class, IDomainEvent
        {
            ulong elapsedTime = 0;
            const int threadWaitingTime = 50;
            while (EventAwaiter<T>.Instance?.Event == null && elapsedTime < timeout)
            {
                Thread.Sleep(threadWaitingTime); // check toutes les 50 ms
                elapsedTime += threadWaitingTime;
            }
            return EventAwaiter<T>.Instance?.Event;
        }

        #endregion

    }
}
