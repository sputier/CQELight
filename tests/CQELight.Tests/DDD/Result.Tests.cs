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
        
    }
}
