using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.MVVM
{
    /// <summary>
    /// Type of alerts.
    /// </summary>
    public enum AlertType
    {
        /// <summary>
        /// Information.
        /// </summary>
        Info,
        /// <summary>
        /// Warning.
        /// </summary>
        Warning,
        /// <summary>
        /// Error.
        /// </summary>
        Error
    }

    /// <summary>
    /// Options for displaying message dialogs.
    /// </summary>
    public class MessageDialogServiceOptions
    {

        #region Properties
        
        /// <summary>
        /// Style de dialogue à afficher.
        /// </summary>
        public AlertType DialogStyle { get; set; } = AlertType.Info;
        
        #endregion

    }
}
