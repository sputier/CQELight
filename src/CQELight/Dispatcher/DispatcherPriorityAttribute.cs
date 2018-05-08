using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Dispatcher
{
    /// <summary>
    /// Attribute for defining an handling order.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DispatcherPriorityAttribute : Attribute
    {
        #region Properties

        /// <summary>
        /// Priority to give.
        /// </summary>
        public byte Priority { get; }

        #endregion

        #region Ctor

        /// <summary>
        /// Attribute ctor.
        /// </summary>
        /// <param name="priority">Current value of the priority.</param>
        public DispatcherPriorityAttribute(byte priority)
        {
            Priority = priority;
        }

        #endregion

    }
}
