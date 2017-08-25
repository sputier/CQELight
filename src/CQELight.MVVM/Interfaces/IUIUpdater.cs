using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.MVVM.Interfaces
{
    /// <summary>
    /// Contract interface for update GUI objects.
    /// </summary>
    public interface IUIUpdater
    {
        /// <summary>
        /// Do an action on the current UI thread.
        /// </summary>
        /// <param name="act">Action to do.</param>
        void PerformOnUIThread(Action act);
    }
}
