using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions
{
    /// <summary>
    /// Base interface for Commands.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Command Id.
        /// </summary>
        Guid Id { get; }
    }
}
