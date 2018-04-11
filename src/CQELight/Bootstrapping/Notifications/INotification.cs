using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Bootstrapping.Notifications
{
    /// <summary>
    /// Enumeration of notification type
    /// </summary>
    public enum BootstrapperNotificationType
    {
        Warning
    }
    /// <summary>
    /// Enumeration of notification content type.
    /// </summary>
    public enum BootstapperNotificationContentType
    {
        IoCServiceMissing,
        DALServiceMissing,
        EventStoreServiceMissing,
        BusServiceMissing
    }

    /// <summary>
    /// Notification of bootstrapper.
    /// </summary>
    public class BootstrapperNotification
    {
        /// <summary>
        /// Type of notification.
        /// </summary>
        public BootstrapperNotificationType Type { get; internal set; }
        /// <summary>
        /// Content type of the notification.
        /// </summary>
        public BootstapperNotificationContentType ContentType { get; internal set; }

    }
}
