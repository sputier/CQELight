using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.MVVM.Interfaces
{
    /// <summary>
    /// Contract interface for closeable view.
    /// </summary>
    public interface ICloseableWindow
    {
        /// <summary>
        /// Close the view.
        /// </summary>
        void Close();
    }
}
