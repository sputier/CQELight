using CQELight.Abstractions.CQS;
using CQELight.Abstractions.DDD;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CQELight.Tests.DDD
{
    public class ResultTests : BaseUnitTestClass
    {

        #region Ctor & members

        #endregion

        #region Fail

        [Fact]
        public void Result_Fail()
        {
            var r = Result.Fail();

            r.IsSuccess.Should().BeFalse();

            var r2 = Result<DateTime>.Fail(DateTime.Now);

            r2.IsSuccess.Should().BeFalse();
            r2.Value.Should().BeSameDateAs(DateTime.Today);

            var r3 = Result.Fail(DateTime.Now);

            r3.IsSuccess.Should().BeFalse();
            r3.Value.Should().BeSameDateAs(DateTime.Today);
        }

        #endregion

        #region Succes

        [Fact]
        public void Result_Success()
        {
            var r = Result.Ok();
            r.IsSuccess.Should().BeTrue();

            var r2 = Result<DateTime>.Ok(DateTime.Today);

            r2.IsSuccess.Should().BeTrue();
            r2.Value.Should().BeSameDateAs(DateTime.Today);

            var r3 = Result.Ok(DateTime.Now);

            r3.IsSuccess.Should().BeTrue();
            r3.Value.Should().BeSameDateAs(DateTime.Today);
        }

        #endregion

        #region Implicit operator

        [Fact]
        public void Result_Implicit_Should_BeTestable()
        {
            var ok = Result.Ok();

            (ok == true).Should().BeTrue();

            var fail = Result.Fail();

            (fail == true).Should().BeFalse();
        }

        #endregion

        #region Combine

        [Fact]
        public void Combine_With_Null_Should_Return_CurrentInstance()
        {
            var r = Result.Ok();
            var res = r.Combine(null);
            res.IsSuccess.Should().BeTrue();

            var r2 = Result.Fail();
            var res2 = r2.Combine(null);
            res2.IsSuccess.Should().BeFalse();

            var r3 = Result.Ok(3);
            var res3 = r3.Combine(null);
            res3.IsSuccess.Should().BeTrue();

            var r4 = Result.Fail(3);
            var res4 = r4.Combine(null);
            res4.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Combine_With_Only_Ok_Should_Returns_Ok()
        {
            var r = Result.Ok();
            var res = r.Combine(Result.Ok(), Result.Ok(), Result.Ok());
            res.IsSuccess.Should().BeTrue();

            var r2 = Result.Ok(42);
            var res2 = r2.Combine(Result.Ok(1), Result.Ok(2), Result.Ok(3));
            res2.IsSuccess.Should().BeTrue();
            res2.Value.As<IEnumerable<int>>().Should().Contain(42);
            res2.Value.As<IEnumerable<int>>().Should().Contain(1);
            res2.Value.As<IEnumerable<int>>().Should().Contain(2);
            res2.Value.As<IEnumerable<int>>().Should().Contain(3);
        }

        [Fact]
        public void Combine_With_One_Fail_Should_Returns_Ok()
        {
            var r = Result.Ok();
            var res = r.Combine(Result.Ok(), Result.Fail(), Result.Ok());
            res.IsSuccess.Should().BeFalse();

            var r2 = Result.Ok(42);
            var res2 = r2.Combine(Result.Ok(1), Result.Fail(2), Result.Ok(3));
            res2.IsSuccess.Should().BeFalse();
            res2.Value.As<IEnumerable<int>>().Should().Contain(42);
            res2.Value.As<IEnumerable<int>>().Should().Contain(1);
            res2.Value.As<IEnumerable<int>>().Should().Contain(2);
            res2.Value.As<IEnumerable<int>>().Should().Contain(3);
        }

        #endregion

    }
}
