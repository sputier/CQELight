using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Buses.AzureServiceBus.Client
{

    class AzureServiceBusClient : IDomainEventBus
    {

        #region IDomainEventBus methods


        public Task RegisterAsync(IDomainEvent @event, IEventContext context = null)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
