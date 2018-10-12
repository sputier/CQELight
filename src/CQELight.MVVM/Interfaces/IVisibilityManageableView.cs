using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.MVVM.Interfaces
{
    /// <summary>
    /// Contract interface for managing view visibility.
    /// </summary>
    public interface IVisibilityManageableView
    {
        /// <summary>
        /// Ask to hide view.
        /// </summary>
        void HideView();
        /// <summary>
        /// Ask to show view.
        /// </summary>
        void ShowView();

    }
}
