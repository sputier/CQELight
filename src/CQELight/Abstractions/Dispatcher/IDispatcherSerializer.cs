using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.Dispatcher
{
    /// <summary>
    /// A contract interface for dispatcher serializer.
    /// </summary>
    public interface IDispatcherSerializer : IEventSerializer, ICommandSerializer
    {
        /// <summary>
        /// Retrieve the content type of serialized data.
        /// </summary>
        string ContentType { get; }
    }
}
