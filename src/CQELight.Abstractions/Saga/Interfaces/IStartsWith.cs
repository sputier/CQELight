using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.Saga.Interfaces
{
    /// <summary>
    /// Contract interface for specific command type to begin a saga.
    /// </summary>
    /// <typeparam name="TCommand">Command type that begins saga.</typeparam>
    public interface IStartsWith<TCommand> : ICommandHandler<TCommand> where TCommand : ICommand
    {
    }
}
