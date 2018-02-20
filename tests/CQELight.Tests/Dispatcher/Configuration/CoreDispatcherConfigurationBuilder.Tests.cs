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
            AddRegistrationFor<JsonEventSerializer>(new JsonEventSerializer());
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
                .SerializeWith<JsonEventSerializer>();

            var cfg = cfgBuilder.Build();

            cfg.EventDispatchersConfiguration.Should().HaveCount(1);
            var dispatch = cfg.EventDispatchersConfiguration.First();

            dispatch.Key.Should().Be(typeof(TestDomainEvent));
            dispatch.Value.Should().HaveCount(1);

            var dispatcher = dispatch.Value.First();

            dispatcher.BusType.Should().Be(typeof(InMemoryEventBus));
            dispatcher.ErrorHandler.Should().NotBeNull();
            dispatcher.Serializer.Should().NotBeNull();
        }

        [Fact]
        public void CoreDispatcherConfigurationBuilder_ForEvent_AllBuses_AsExpected()
        {
            var cfgBuilder = new CoreDispatcherConfigurationBuilder();
            cfgBuilder
                .ForEvent<TestDomainEvent>()
                .UseAllAvailableBuses()
                .HandleErrorWith(e => _error = e.ToString())
                .SerializeWith<JsonEventSerializer>();

            var cfg = cfgBuilder.Build();

            cfg.EventDispatchersConfiguration.Should().HaveCount(1);
            var dispatch = cfg.EventDispatchersConfiguration.First();

            dispatch.Key.Should().Be(typeof(TestDomainEvent));
            dispatch.Value.Should().HaveSameCount(ReflectionTools.GetAllTypes()
                .Where(t => typeof(IDomainEventBus).IsAssignableFrom(t) && t.IsClass));

            dispatch.Value.All(d => d.ErrorHandler != null).Should().BeTrue();
            dispatch.Value.All(d => d.Serializer != null).Should().BeTrue();

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
                .SerializeWith<JsonEventSerializer>();

            var cfg = cfgBuilder.Build();

            cfg.EventDispatchersConfiguration.Should().HaveCount(ReflectionTools.GetAllTypes()
                .Count(t => typeof(IDomainEvent).IsAssignableFrom(t) && t.IsClass));
            var dispatch = cfg.EventDispatchersConfiguration.First(t => t.Key == typeof(TestDomainEvent));
            dispatch.Should().NotBeNull();
            dispatch.Key.Should().Be(typeof(TestDomainEvent));
            dispatch.Value.Should().HaveCount(1);

            var dispatcher = dispatch.Value.First();

            dispatcher.BusType.Should().Be(typeof(InMemoryEventBus));
            dispatcher.ErrorHandler.Should().NotBeNull();
            dispatcher.Serializer.Should().NotBeNull();
        }

        [Fact]
        public void CoreDispatcherConfigurationBuilder_ForAllEvents_AllBuses_AsExpected()
        {
            var cfgBuilder = new CoreDispatcherConfigurationBuilder();
            cfgBuilder
                .ForAllEvents()
                .UseAllAvailableBuses()
                .HandleErrorWith(e => _error = e.ToString())
                .SerializeWith<JsonEventSerializer>();

            var cfg = cfgBuilder.Build();

            cfg.EventDispatchersConfiguration.Should().HaveCount(ReflectionTools.GetAllTypes()
                .Count(t => typeof(IDomainEvent).IsAssignableFrom(t) && t.IsClass));
            var dispatch = cfg.EventDispatchersConfiguration.First(t => t.Key == typeof(TestDomainEvent));
            dispatch.Should().NotBeNull();
            dispatch.Key.Should().Be(typeof(TestDomainEvent));
            dispatch.Value.Should().HaveSameCount(ReflectionTools.GetAllTypes()
                .Where(t => typeof(IDomainEventBus).IsAssignableFrom(t) && t.IsClass));

            dispatch.Value.All(d => d.ErrorHandler != null).Should().BeTrue();
            dispatch.Value.All(d => d.Serializer != null).Should().BeTrue();
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
                .SerializeWith<JsonEventSerializer>();

            cfgBuilder
                .ForAllOtherEvents()
                .UseBus<InMemoryEventBus>()
                .HandleErrorWith(e => _error = e.ToString())
                .SerializeWith<JsonEventSerializer>();

            var cfg = cfgBuilder.Build();

            cfg.EventDispatchersConfiguration.Should().HaveCount(ReflectionTools.GetAllTypes().Count(t => typeof(IDomainEvent).IsAssignableFrom(t) && t.IsClass));
            var dispatch = cfg.EventDispatchersConfiguration.First(t => t.Key == typeof(TestDomainEvent));
            dispatch.Should().NotBeNull();
            dispatch.Key.Should().Be(typeof(TestDomainEvent));
            dispatch.Value.Should().HaveCount(1);

            var dispatcher = dispatch.Value.First();

            dispatcher.BusType.Should().Be(typeof(InMemoryEventBus));
            dispatcher.ErrorHandler.Should().NotBeNull();
            dispatcher.Serializer.Should().NotBeNull();

            dispatch = cfg.EventDispatchersConfiguration.First(t => t.Key == typeof(SecondTestDomainEvent));
            dispatch.Should().NotBeNull();
            dispatch.Key.Should().Be(typeof(SecondTestDomainEvent));
            dispatch.Value.Should().HaveCount(1);

            dispatcher = dispatch.Value.First();

            dispatcher.BusType.Should().Be(typeof(InMemoryEventBus));
            dispatcher.ErrorHandler.Should().NotBeNull();
            dispatcher.Serializer.Should().NotBeNull();
        }

        [Fact]
        public void CoreDispatcherConfigurationBuilder_ForAllOtherEvents_AllBuses_AsExpected()
        {
            var cfgBuilder = new CoreDispatcherConfigurationBuilder();
            cfgBuilder
                .ForEvent<TestDomainEvent>()
                .UseBus<InMemoryEventBus>()
                .HandleErrorWith(e => _error = e.ToString())
                .SerializeWith<JsonEventSerializer>();

            cfgBuilder
                .ForAllOtherEvents()
                .UseAllAvailableBuses()
                .HandleErrorWith(e => _error = e.ToString())
                .SerializeWith<JsonEventSerializer>();

            var cfg = cfgBuilder.Build();

            cfg.EventDispatchersConfiguration.Should().HaveCount(ReflectionTools.GetAllTypes().Count(t => typeof(IDomainEvent).IsAssignableFrom(t) && t.IsClass));
            var dispatch = cfg.EventDispatchersConfiguration.First(t => t.Key == typeof(TestDomainEvent));
            dispatch.Should().NotBeNull();
            dispatch.Key.Should().Be(typeof(TestDomainEvent));
            dispatch.Value.Should().HaveCount(1);

            var dispatcher = dispatch.Value.First();

            dispatcher.BusType.Should().Be(typeof(InMemoryEventBus));
            dispatcher.ErrorHandler.Should().NotBeNull();
            dispatcher.Serializer.Should().NotBeNull();

            dispatch = cfg.EventDispatchersConfiguration.First(t => t.Key == typeof(SecondTestDomainEvent));
            dispatch.Should().NotBeNull();
            dispatch.Key.Should().Be(typeof(SecondTestDomainEvent));
            dispatch.Value.Should().HaveSameCount(ReflectionTools.GetAllTypes().Where(t => typeof(IDomainEventBus).IsAssignableFrom(t) && t.IsClass));

            dispatcher = dispatch.Value.First(t => t.BusType == typeof(InMemoryEventBus));

            dispatcher.BusType.Should().Be(typeof(InMemoryEventBus));
            dispatcher.ErrorHandler.Should().NotBeNull();
            dispatcher.Serializer.Should().NotBeNull();
        }


        #endregion

        #region ValidateStrict

        [Fact]
        public void CoreDispatcherConfigurationBuilder_ValidateStrict_Error()
        {
            var cfgBuilder = new CoreDispatcherConfigurationBuilder();
            cfgBuilder
                 .ForEvent<TestDomainEvent>()
                 .UseAllAvailableBuses()
                 .HandleErrorWith(e => _error = e.ToString())
                 .SerializeWith<JsonEventSerializer>();

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
                .SerializeWith<JsonEventSerializer>();

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
                 .SerializeWith<JsonEventSerializer>();

            var c = cfgBuilder.Build(false);
            c.ValidateStrict().Should().BeTrue();
        }

        #endregion

    }
}
