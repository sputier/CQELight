using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Buses.InMemory.Commands;
using CQELight.Dispatcher;
using CQELight.TestFramework;
using CQELight.TestFramework.IoC;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.Buses.InMemory.Integration.Tests
{
    public class InMemoryCommandBusTests : BaseUnitTestClass
    {

        #region Ctor & members

        private class TestNotFoundCommand : ICommand { }

        private class TestCommand : ICommand
        {
            public string Data { get; set; }
        }

        private class TestCommandHandler : ICommandHandler<TestCommand>
        {
            public static string HandlerData { get; private set; }
            public static string Origin { get; private set; }
            public static void ResetTestData()
                => HandlerData = string.Empty;

            public TestCommandHandler(string data)
            {
                ResetTestData();
                Origin = "spec_ctor";
            }
            public TestCommandHandler()
            {
                ResetTestData();
                Origin = "reflexion";
            }
            public Task HandleAsync(TestCommand command, ICommandContext context = null)
            {
                HandlerData = command.Data;
                return Task.CompletedTask;
            }
        }

        private class TestIfCommand : ICommand
        {
            public int Data { get; set; }
        }

        private class TestIfCommandHandler : ICommandHandler<TestIfCommand>
        {
            public static int Data { get; private set; }
            public static void ResetData() => Data = 0;
            public Task HandleAsync(TestIfCommand command, ICommandContext context = null)
            {
                Data = command.Data;
                return Task.CompletedTask;
            }
        }

        private class TestMultipleHandlerIllicit : ICommand { }
        private class TestMultipleHandlerIllicitHandlerOne : ICommandHandler<TestMultipleHandlerIllicit>
        {
            public Task HandleAsync(TestMultipleHandlerIllicit command, ICommandContext context = null)
            {
                throw new NotImplementedException();
            }
        }
        private class TestMultipleHandlerIllicitHandlerTwo : ICommandHandler<TestMultipleHandlerIllicit>
        {
            public Task HandleAsync(TestMultipleHandlerIllicit command, ICommandContext context = null)
            {
                throw new NotImplementedException();
            }
        }
        private class TestMultipleHandlerFromConfig : ICommand { }
        private class TestMultipleHandlerFromConfigHandlerOne : ICommandHandler<TestMultipleHandlerFromConfig>
        {
            public Task HandleAsync(TestMultipleHandlerFromConfig command, ICommandContext context = null)
                => Task.CompletedTask;
        }
        private class TestMultipleHandlerFromConfigHandlerTwo : ICommandHandler<TestMultipleHandlerFromConfig>
        {
            public Task HandleAsync(TestMultipleHandlerFromConfig command, ICommandContext context = null)
                => Task.CompletedTask;
        }

        public InMemoryCommandBusTests()
        {
            TestCommandHandler.ResetTestData();
        }

        #endregion

        #region DispatchAsync

        [Fact]
        public async Task InMemoryCommandBus_DispatchAsync_HandlerFromIoC()
        {
            var factory = new TestScopeFactory();
            factory.Instances.Add(typeof(ICommandHandler<TestCommand>), new TestCommandHandler("tt"));

            CleanRegistrationInDispatcher();
            var bus = new InMemoryCommandBus(scopeFactory: factory);

            var tasks = await bus.DispatchAsync(new TestCommand { Data = "test_ioc" }).ConfigureAwait(false);

            tasks.Should().HaveCount(2);

            await Task.WhenAll(tasks).ConfigureAwait(false);

            TestCommandHandler.HandlerData.Should().Be("test_ioc");
            TestCommandHandler.Origin.Should().Be("spec_ctor");
        }


        [Fact]
        public async Task InMemoryCommandBus_DispatchAsync_FromCoreDispatcher()
        {
            CleanRegistrationInDispatcher();
            CoreDispatcher.AddHandlerToDispatcher(new TestCommandHandler("coreDispatcher"));
            var bus = new InMemoryCommandBus();

            var tasks = await bus.DispatchAsync(new TestCommand { Data = "test_dispatcher" }).ConfigureAwait(false);

            tasks.Should().HaveCount(2);

            await Task.WhenAll(tasks).ConfigureAwait(false);

            TestCommandHandler.HandlerData.Should().Be("test_dispatcher");
            TestCommandHandler.Origin.Should().Be("spec_ctor");
        }



        [Fact]
        public async Task InMemoryCommandBus_DispatchAsync_Reflexion()
        {
            CleanRegistrationInDispatcher();
            var bus = new InMemoryCommandBus();

            var tasks = await bus.DispatchAsync(new TestCommand { Data = "test_ioc" }).ConfigureAwait(false);

            tasks.Should().HaveCount(2);

            await Task.WhenAll(tasks).ConfigureAwait(false);

            TestCommandHandler.HandlerData.Should().Be("test_ioc");
            TestCommandHandler.Origin.Should().Be("reflexion");
        }

        [Fact]
        public async Task InMemoryCommandBus_DispatchAsync_NoHandlerFound()
        {
            var hInvoked = false;
            var c = new InMemoryCommandBusConfigurationBuilder().AddHandlerWhenHandlerIsNotFound((cmd, ctx) => hInvoked = true).Build();
            var bus = new InMemoryCommandBus(c);

            var tasks = await bus.DispatchAsync(new TestNotFoundCommand()).ConfigureAwait(false);

            tasks.Should().HaveCount(1);

            await Task.WhenAll(tasks).ConfigureAwait(false);

            hInvoked.Should().BeTrue();
        }

        [Fact]
        public async Task InMemoryCommandBus_DispatchAsync_MultipleHandlersShould_Throw_Exception()
            => await Assert.ThrowsAsync<InvalidOperationException>(() => new InMemoryCommandBus().DispatchAsync(new TestMultipleHandlerIllicit())).ConfigureAwait(false);

        #endregion

        #region Configuration


        [Fact]
        public async Task InMemoryCommandBus_Configuration_DispatchIfClause()
        {
            TestIfCommandHandler.ResetData();
            var cfgBuilder =
                new InMemoryCommandBusConfigurationBuilder()
                .DispatchOnlyIf<TestIfCommand>(e => e.Data > 1);

            var b = new InMemoryCommandBus(cfgBuilder.Build());

            TestIfCommandHandler.Data.Should().Be(0);

            await b.DispatchAsync(new TestIfCommand { Data = 1 }).ConfigureAwait(false);

            TestIfCommandHandler.Data.Should().Be(0);

            await b.DispatchAsync(new TestIfCommand { Data = 10 }).ConfigureAwait(false);

            TestIfCommandHandler.Data.Should().Be(10);

        }

        [Fact]
        public async Task InMemoryCommandBus_Configuration_MultipleHandlers_ConfigurationOk()
        {
            var c = new InMemoryCommandBusConfigurationBuilder()
                .AllowMultipleHandlers<TestMultipleHandlerFromConfig>();
            var bus = new InMemoryCommandBus(c.Build());

            var tasks = await bus.DispatchAsync(new TestMultipleHandlerFromConfig()).ConfigureAwait(false);

            tasks.Should().HaveCount(3);

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        #endregion

    }
}
