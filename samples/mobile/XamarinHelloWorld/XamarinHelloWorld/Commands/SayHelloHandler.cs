using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.DDD;
using CQELight.Dispatcher;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XamarinHelloWorld.Events;

namespace XamarinHelloWorld.Commands
{
    class SayHelloHandler : ICommandHandler<SayHello>
    {
        public async Task<Result> HandleAsync(SayHello command, ICommandContext context = null)
        {
            await CoreDispatcher.PublishEventAsync(new HelloSaid());
            return Result.Ok();
        }
    }
}
