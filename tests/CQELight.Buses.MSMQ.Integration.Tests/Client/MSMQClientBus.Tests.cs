using CQELight.Abstractions.Configuration;
using CQELight.Abstractions.Events;
using CQELight.Buses.MSMQ.Client;
using CQELight.Configuration;
using CQELight.Events.Serializers;
using CQELight.TestFramework;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.Buses.MSMQ.Integration.Tests.Client
{
    public class MSMQClientBusTests : BaseUnitTestClass
    {

        #region Ctor & members

        private class MSMQEvent : BaseDomainEvent
        {
            public string Data { get; set; }
        }

        private const string CONST_APP_ID = "AA3F9093-D7EE-4BB8-9B4E-EEC3447A89BA";

        private AppId _appId;
        private readonly Mock<IAppIdRetriever> _appIdRetrieverMock;

        public MSMQClientBusTests()
        {
            CleanQueues();

            _appId = new AppId(Guid.Parse(CONST_APP_ID));
            _appIdRetrieverMock = new Mock<IAppIdRetriever>();
            _appIdRetrieverMock.Setup(m => m.GetAppId()).Returns(_appId);
        }
        private MessageQueue GetQueue() => new MessageQueue($@".\Private$\CQELight_{CONST_APP_ID}");

        private void CleanQueues()
        {
            if(!MessageQueue.Exists($@".\Private$\CQELight_{CONST_APP_ID}"))
            {
                MessageQueue.Create($@".\Private$\CQELight_{CONST_APP_ID}");
            }
            MessageQueue q = GetQueue();
            if (q != null)
            {
                var enumerator = q.GetMessageEnumerator2();
                var filter = new MessagePropertyFilter();
                filter.ClearAll();
                filter.ArrivedTime = true;
                q.MessageReadPropertyFilter = filter;

                while (enumerator.MoveNext())
                {
                    enumerator.RemoveCurrent();
                }

;
            }
        }

        #endregion

        #region RegisterEvent

        [Fact]
        public async Task MSMQClientBus_RegisterAsync_AsExpected()
        {
            var evt = new MSMQEvent
            {
                Data = "testData"
            };

            var b = new MSMQClientBus(
                _appIdRetrieverMock.Object,
                new JsonDispatcherSerializer(),
                new MSMQClientBusConfiguration());

            await b.RegisterAsync(evt).ConfigureAwait(false);

            var q = GetQueue();

            var messages = q.GetAllMessages();
            messages.Should().NotBeEmpty();

        }

        #endregion

    }
}
