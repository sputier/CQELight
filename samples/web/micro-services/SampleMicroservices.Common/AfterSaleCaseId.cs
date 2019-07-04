using System;
using System.Collections.Generic;
using System.Text;

namespace SampleMicroservices.Common
{
    public struct AfterSaleCaseId
    {
        #region Properties

        public Guid Value { get; private set; }

        #endregion

        #region Ctor

        public AfterSaleCaseId(Guid value)
        {
            if(value == Guid.Empty)
            {
                throw new ArgumentException("AfterSaleCaseId.ctor : Value is incorrect (cannot be empty)");
            }
            Value = value;
        }

        #endregion

        #region Public static methods

        public static AfterSaleCaseId Generate() => new AfterSaleCaseId(Guid.NewGuid());

        #endregion

    }
}
