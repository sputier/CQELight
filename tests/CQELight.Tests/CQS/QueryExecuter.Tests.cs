using CQELight.Abstractions.CQS.Interfaces;
using CQELight.CQS;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.Tests.CQS
{

    public class QueryExecuterTests : BaseUnitTestClass
    {

        #region Ctor & members

        private class SimpleQuery : IQuery<int>
        {
            public Task<int> ExecuteQueryAsync()
            {
                return Task.FromResult(42);
            }
        }

        private class SimpleParameterizedQuery : IQuery<int, int>
        {
            public Task<int> ExecuteQueryAsync(int param)
            {
                return Task.FromResult(param / 2);
            }
        }

        private class NoEmptyCtorQuery : IQuery<int>
        {
            private readonly string _data;

            public NoEmptyCtorQuery(string data)
            {
                _data = data;
            }

            public Task<int> ExecuteQueryAsync()
                => Task.FromResult(60);
        }

        public QueryExecuterTests()
            : base(true)
        {

        }

        #endregion

        #region ExecuteQueryAsync

        [Fact]
        public async Task ExecuteQueryAsync_NoIoC_AsExpected_Should_Returns_Result()
        {
            var result = await QueryExecuter.ExecuteQueryAsync<SimpleQuery, int>();

            result.Should().Be(42);
        }

        [Fact]
        public async Task ExecuteQueryAsync_NoIoC_WithParam_AsExpected_Should_Returns_Result()
        {
            var result = await QueryExecuter.ExecuteQueryAsync<SimpleParameterizedQuery, int, int>(40);

            result.Should().Be(20);
        }

        [Fact]
        public async Task ExecuteQueryAsync_NoIoC_WithCtorParam_Should_Returns_DefaultValue()
        {
            var result = await QueryExecuter.ExecuteQueryAsync<NoEmptyCtorQuery, int>();

            result.Should().Be(0);
        }

        #endregion

    }
}
