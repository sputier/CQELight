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
        /// <summary>
        /// Notification of type 'Warning'.
        /// </summary>
        Warning
    }
    /// <summary>
    /// Enumeration of notification content type.
    /// </summary>
    public enum BootstapperNotificationContentType
    {
        /// <summary>
        /// Type of notification when IoC service is not registered.
        /// </summary>
        IoCServiceMissing,
        /// <summary>
        /// Type of notification when DAL service is not registered.
        /// </summary>
        DALServiceMissing,
        /// <summary>
        /// Type of notification when EventStore service is not registered.
        /// </summary>
        EventStoreServiceMissing,
        /// <summary>
        /// Type of notification when Bus service is not registered.
        /// </summary>
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
