using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.Attributes
{
    /// <summary>
    /// Attribute used to declare that a specific kind of event should not be persisted in database 
    /// if EventStore is used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EventNotPersistedAttribute : Attribute
    {
    }
}
