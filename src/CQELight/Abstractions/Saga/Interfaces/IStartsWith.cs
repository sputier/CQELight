using CQELight.Abstractions.CQS.Interfaces;

namespace CQELight.Abstractions.Saga.Interfaces
{
    /// <summary>
    /// Contract interface for specific command type to begin a saga.
    /// </summary>
    /// <typeparam name="TCommand">Command type that begins saga.</typeparam>
    public interface IStartsWith<in TCommand> : ICommandHandler<TCommand> where TCommand : ICommand
    {
    }
}
