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
        private class TestNotFoundCommandHandler : ICommandHandler<TestNotFoundCommand>
        {
            public TestNotFoundCommandHandler(string data)
            {

            }
            public Task HandleAsync(TestNotFoundCommand command, ICommandContext context = null)
            {
                throw new NotImplementedException();
            }
        }

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
            var bus = new InMemoryCommandBus(factory);

            var tasks = await bus.DispatchAsync(new TestCommand { Data = "test_ioc" });

            tasks.Should().HaveCount(2);

            await Task.WhenAll(tasks);

            TestCommandHandler.HandlerData.Should().Be("test_ioc");
            TestCommandHandler.Origin.Should().Be("spec_ctor");
        }


        [Fact]
        public async Task InMemoryCommandBus_DispatchAsync_FromCoreDispatcher()
        {
            CleanRegistrationInDispatcher();
            CoreDispatcher.AddHandlerToDispatcher(new TestCommandHandler("coreDispatcher"));
            var bus = new InMemoryCommandBus();

            var tasks = await bus.DispatchAsync(new TestCommand { Data = "test_dispatcher" });

            tasks.Should().HaveCount(2);

            await Task.WhenAll(tasks);

            TestCommandHandler.HandlerData.Should().Be("test_dispatcher");
            TestCommandHandler.Origin.Should().Be("spec_ctor");
        }



        [Fact]
        public async Task InMemoryCommandBus_DispatchAsync_Reflexion()
        {
            CleanRegistrationInDispatcher();
            var bus = new InMemoryCommandBus();

            var tasks = await bus.DispatchAsync(new TestCommand { Data = "test_ioc" });

            tasks.Should().HaveCount(2);

            await Task.WhenAll(tasks);

            TestCommandHandler.HandlerData.Should().Be("test_ioc");
            TestCommandHandler.Origin.Should().Be("reflexion");
        }

        [Fact]
        public async Task InMemoryCommandBus_DispatchAsync_NoHandlerFound()
        {
            var hInvoked = false;
            var c = new InMemoryCommandBusConfiguration((cmd, ctx) => hInvoked = true);
            var bus = new InMemoryCommandBus();
            bus.Configure(c);

            var tasks = await bus.DispatchAsync(new TestNotFoundCommand());

            tasks.Should().HaveCount(1);

            await Task.WhenAll(tasks);

            hInvoked.Should().BeTrue();
        }

        #endregion

    }
}
