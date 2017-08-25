using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.MVVM.Interfaces
{
    /// <summary>
    /// Contract interface for view
    /// </summary>
    public interface IView : ICloseableWindow, IMessageDialogService, IUIUpdater, ILoadingPanelDisplayer
    {
        /// <summary>
        /// Show a new window as popup of this one.
        /// </summary>
        /// <param name="popupWindow">Popup to display.</param>
        void ShowPopup(IView popupWindow);
    }
}
