using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.DAL.Interfaces;
using CQELight.Examples.Console.Events;
using CQELight.Examples.Console.Models.DbEntities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Examples.Console.Handlers.Events
{
    /// <summary>
    /// This handler has been added after business folks asks to log every received message into database,
    /// in order to have an history of them.
    /// </summary>
    class MessageTreated_Database_EventHandler : IDomainEventHandler<MessageTreatedEvent>, IAutoRegisterType
    {

        #region Members

        private readonly IDataUpdateRepository<DbMessage> _messageRepository;

        #endregion

        #region Ctor

        public MessageTreated_Database_EventHandler(IDataUpdateRepository<DbMessage> messageRepository)
        {
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        }

        #endregion

        #region IDomainEventHandler methods

        public async Task HandleAsync(MessageTreatedEvent domainEvent, IEventContext context = null)
        {
            var dbMessage = new DbMessage(domainEvent.TreatedMessageId)
            {
                Message = domainEvent.TreatedMessage
            };

            _messageRepository.MarkForInsert(dbMessage);
            await _messageRepository.SaveAsync();
        }

        #endregion

    }
}
