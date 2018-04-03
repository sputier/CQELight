using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.Tests.DDD;
using CQELight.Buses.InMemory.Events;
using CQELight.Dispatcher.Configuration;
using CQELight.Events.Serializers;
using CQELight.TestFramework;
using CQELight.Tools;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace CQELight.Tests.Dispatcher.Configuration
{
    public class CoreDispatcherConfigurationBuilderTests : BaseUnitTestClass
    {

        #region Ctor & members

        #region Nested classes & namespaces

        internal class SecondTestDomainEvent : BaseDomainEvent
        {
        }

        #endregion

        private string _error;

        public CoreDispatcherConfigurationBuilderTests()
        {
            _error = string.Empty;
            AddRegistrationFor<InMemoryEventBus>(new InMemoryEventBus());
            AddRegistrationFor<JsonDispatcherSerializer>(new JsonDispatcherSerializer());
        }

        #endregion

        #region ForEvent

        [Fact]
        public void CoreDispatcherConfigurationBuilder_ForEvent_SingleBus_AsExpected()
        {
            var cfgBuilder = new CoreDispatcherConfigurationBuilder();
            cfgBuilder
                .ForEvent<TestDomainEvent>()
                .UseBus<InMemoryEventBus>()
                .HandleErrorWith(e => _error = e.ToString())
                .SerializeWith<JsonDispatcherSerializer>();

            var cfg = cfgBuilder.Build();

            cfg.EventDispatchersConfiguration.Should().HaveCount(1);
            var dispatch = cfg.EventDispatchersConfiguration.First();

            dispatch.EventType.Should().Be(typeof(TestDomainEvent));
            dispatch.BusesTypes.Should().HaveCount(1);
            dispatch.ErrorHandler.Should().NotBeNull();
            dispatch.Serializer.Should().NotBeNull();

            var dispatcher = dispatch.BusesTypes.First();

            dispatcher.Should().Be(typeof(InMemoryEventBus));
        }

        [Fact]
        public void CoreDispatcherConfigurationBuilder_ForEvent_AllBuses_AsExpected()
        {
            var cfgBuilder = new CoreDispatcherConfigurationBuilder();
            cfgBuilder
                .ForEvent<TestDomainEvent>()
                .UseAllAvailableBuses()
                .HandleErrorWith(e => _error = e.ToString())
                .SerializeWith<JsonDispatcherSerializer>();

            var cfg = cfgBuilder.Build();

            cfg.EventDispatchersConfiguration.Should().HaveCount(1);
            var dispatch = cfg.EventDispatchersConfiguration.First();

            dispatch.EventType.Should().Be(typeof(TestDomainEvent));
            dispatch.BusesTypes.Should().HaveSameCount(ReflectionTools.GetAllTypes()
                .Where(t => typeof(IDomainEventBus).IsAssignableFrom(t) && t.IsClass));

            dispatch.ErrorHandler.Should().NotBeNull();
            dispatch.Serializer.Should().NotBeNull();

        }

        #endregion

        #region ForAllEvents

        [Fact]
        public void CoreDispatcherConfigurationBuilder_ForAllEvents_SingleBus_AsExpected()
        {
            var cfgBuilder = new CoreDispatcherConfigurationBuilder();
            cfgBuilder
                .ForAllEvents()
                .UseBus<InMemoryEventBus>()
                .HandleErrorWith(e => _error = e.ToString())
                .SerializeWith<JsonDispatcherSerializer>();

            var cfg = cfgBuilder.Build();

            cfg.EventDispatchersConfiguration.Should().HaveCount(ReflectionTools.GetAllTypes()
                .Count(t => typeof(IDomainEvent).IsAssignableFrom(t) && t.IsClass));
            var dispatch = cfg.EventDispatchersConfiguration.First(t => t.EventType == typeof(TestDomainEvent));
            dispatch.Should().NotBeNull();
            dispatch.EventType.Should().Be(typeof(TestDomainEvent));
            dispatch.BusesTypes.Should().HaveCount(1);
            dispatch.ErrorHandler.Should().NotBeNull();
            dispatch.Serializer.Should().NotBeNull();

            var dispatcher = dispatch.BusesTypes.First();

            dispatcher.Should().Be(typeof(InMemoryEventBus));
        }

        [Fact]
        public void CoreDispatcherConfigurationBuilder_ForAllEvents_AllBuses_AsExpected()
        {
            var cfgBuilder = new CoreDispatcherConfigurationBuilder();
            cfgBuilder
                .ForAllEvents()
                .UseAllAvailableBuses()
                .HandleErrorWith(e => _error = e.ToString())
                .SerializeWith<JsonDispatcherSerializer>();

            var cfg = cfgBuilder.Build();

            cfg.EventDispatchersConfiguration.Should().HaveCount(ReflectionTools.GetAllTypes()
                .Count(t => typeof(IDomainEvent).IsAssignableFrom(t) && t.IsClass));
            var dispatch = cfg.EventDispatchersConfiguration.First(t => t.EventType == typeof(TestDomainEvent));
            dispatch.Should().NotBeNull();
            dispatch.EventType.Should().Be(typeof(TestDomainEvent));
            dispatch.BusesTypes.Should().HaveSameCount(ReflectionTools.GetAllTypes()
                .Where(t => typeof(IDomainEventBus).IsAssignableFrom(t) && t.IsClass));

            dispatch.ErrorHandler.Should().NotBeNull();
            dispatch.Serializer.Should().NotBeNull();
        }

        #endregion

        #region ForAllOtherEvents

        [Fact]
        public void CoreDispatcherConfigurationBuilder_ForAllOtherEvents_SingleBus_AsExpected()
        {
            var cfgBuilder = new CoreDispatcherConfigurationBuilder();
            cfgBuilder
                .ForEvent<TestDomainEvent>()
                .UseBus<InMemoryEventBus>()
                .HandleErrorWith(e => _error = e.ToString())
                .SerializeWith<JsonDispatcherSerializer>();

            cfgBuilder
                .ForAllOtherEvents()
                .UseBus<InMemoryEventBus>()
                .HandleErrorWith(e => _error = e.ToString())
                .SerializeWith<JsonDispatcherSerializer>();

            var cfg = cfgBuilder.Build();

            cfg.EventDispatchersConfiguration.Should().HaveCount(ReflectionTools.GetAllTypes().Count(t => typeof(IDomainEvent).IsAssignableFrom(t) && t.IsClass));
            var dispatch = cfg.EventDispatchersConfiguration.First(t => t.EventType == typeof(TestDomainEvent));
            dispatch.Should().NotBeNull();
            dispatch.EventType.Should().Be(typeof(TestDomainEvent));
            dispatch.BusesTypes.Should().HaveCount(1);
            dispatch.ErrorHandler.Should().NotBeNull();
            dispatch.Serializer.Should().NotBeNull();

            var dispatcher = dispatch.BusesTypes.First();

            dispatcher.Should().Be(typeof(InMemoryEventBus));

            dispatch = cfg.EventDispatchersConfiguration.First(t => t.EventType == typeof(SecondTestDomainEvent));
            dispatch.Should().NotBeNull();
            dispatch.EventType.Should().Be(typeof(SecondTestDomainEvent));
            dispatch.BusesTypes.Should().HaveCount(1);
            dispatch.ErrorHandler.Should().NotBeNull();
            dispatch.Serializer.Should().NotBeNull();

            dispatcher = dispatch.BusesTypes.First();

            dispatcher.Should().Be(typeof(InMemoryEventBus));
        }

        [Fact]
        public void CoreDispatcherConfigurationBuilder_ForAllOtherEvents_AllBuses_AsExpected()
        {
            var cfgBuilder = new CoreDispatcherConfigurationBuilder();
            cfgBuilder
                .ForEvent<TestDomainEvent>()
                .UseBus<InMemoryEventBus>()
                .HandleErrorWith(e => _error = e.ToString())
                .SerializeWith<JsonDispatcherSerializer>();

            cfgBuilder
                .ForAllOtherEvents()
                .UseAllAvailableBuses()
                .HandleErrorWith(e => _error = e.ToString())
                .SerializeWith<JsonDispatcherSerializer>();

            var cfg = cfgBuilder.Build();

            cfg.EventDispatchersConfiguration.Should().HaveCount(ReflectionTools.GetAllTypes().Count(t => typeof(IDomainEvent).IsAssignableFrom(t) && t.IsClass));
            var dispatch = cfg.EventDispatchersConfiguration.First(t => t.EventType == typeof(TestDomainEvent));
            dispatch.Should().NotBeNull();
            dispatch.EventType.Should().Be(typeof(TestDomainEvent));
            dispatch.BusesTypes.Should().HaveCount(1);
            dispatch.ErrorHandler.Should().NotBeNull();
            dispatch.Serializer.Should().NotBeNull();

            var dispatcher = dispatch.BusesTypes.First();

            dispatcher.Should().Be(typeof(InMemoryEventBus));

            dispatch = cfg.EventDispatchersConfiguration.First(t => t.EventType == typeof(SecondTestDomainEvent));
            dispatch.Should().NotBeNull();
            dispatch.EventType.Should().Be(typeof(SecondTestDomainEvent));
            dispatch.BusesTypes.Should().HaveSameCount(ReflectionTools.GetAllTypes().Where(t => typeof(IDomainEventBus).IsAssignableFrom(t) && t.IsClass));
            dispatch.ErrorHandler.Should().NotBeNull();
            dispatch.Serializer.Should().NotBeNull();

            dispatcher = dispatch.BusesTypes.First(t => t == typeof(InMemoryEventBus));

            dispatcher.Should().Be(typeof(InMemoryEventBus));
        }


        #endregion

        #region ValidateStrict

        [Fact]
        public void CoreDispatcherConfigurationBuilder_ValidateStrict_Error()
        {
            var cfgBuilder = new CoreDispatcherConfigurationBuilder();
            cfgBuilder
                 .ForEvent<TestDomainEvent>()
                 .HandleErrorWith(e => _error = e.ToString())
                 .SerializeWith<JsonDispatcherSerializer>();

            var c = cfgBuilder.Build(true);
            c.ValidateStrict().Should().BeFalse();
        }

        [Fact]
        public void CoreDispatcherConfigurationBuilder_ValidateStrict_AsExpected()
        {
            var cfgBuilder = new CoreDispatcherConfigurationBuilder();
            cfgBuilder
                .ForAllEvents()
                .UseAllAvailableBuses()
                .HandleErrorWith(e => _error = e.ToString())
                .SerializeWith<JsonDispatcherSerializer>();

            var cfg = cfgBuilder.Build(true);
            cfg.ValidateStrict().Should().BeTrue();
        }

        [Fact]
        public void CoreDispatcherConfigurationBuilder_ValidateStrict_NotStrict()
        {
            var cfgBuilder = new CoreDispatcherConfigurationBuilder();
            cfgBuilder
                 .ForEvent<TestDomainEvent>()
                 .UseAllAvailableBuses()
                 .HandleErrorWith(e => _error = e.ToString())
                 .SerializeWith<JsonDispatcherSerializer>();

            var c = cfgBuilder.Build(false);
            c.ValidateStrict().Should().BeTrue();
        }

        #endregion

    }
}
