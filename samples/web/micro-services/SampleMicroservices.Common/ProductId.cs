using System;
using System.Collections.Generic;
using System.Text;

namespace SampleMicroservices.Common
{
    public struct ProductId
    {
        #region Properties

        public int Value { get; private set; }

        #endregion

        #region Ctor

        public ProductId(int value)
        {
            if (value <= 0)
            {
                throw new ArgumentException("Product id is not valid");
            }
            Value = value;
        }

        #endregion
    }
}
