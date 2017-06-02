using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.IoC.Interfaces
{
    /// <summary>
    /// Definition of a scope to resolve thing during a determined lifetime.
    /// </summary>
    public interface IScope : IDisposable, ITypeResolver, IEventContext, ICommandContext
    {

        /// <summary>
        /// Indicates if scope is disposed or not.
        /// </summary>
        bool IsDisposed { get; }

    }
}
