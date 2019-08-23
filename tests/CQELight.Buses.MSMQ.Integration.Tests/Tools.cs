using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Buses.MSMQ.Integration.Tests
{
    static class Tools
    {

        public static void CleanQueue()
        {
            var queue = Helpers.GetMessageQueue();
            queue.Purge();
        }
    }
}
