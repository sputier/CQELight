using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.InMemory
{
    /// <summary>
    /// Different values of handler priority.
    /// </summary>
    public enum HandlerPriority
    {
        Normal = 0,
        Highest = 2,
        High = 1,
        Low = -1,
        Lowest = -2
    }

    /// <summary>
    /// Attribute for defining an handling order.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class HandlerPriorityAttribute : Attribute
    {
        #region Properties

        /// <summary>
        /// Priority to give.
        /// </summary>
        public HandlerPriority Priority { get; }

        #endregion

        #region Ctor

        /// <summary>
        /// Attribute ctor.
        /// </summary>
        /// <param name="priority">Current value of the priority.</param>
        public HandlerPriorityAttribute(HandlerPriority priority = HandlerPriority.Normal)
        {
            Priority = priority;
        }

        #endregion

    }
}
