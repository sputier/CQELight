using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.Abstractions.DDD
{
    /// <summary>
    /// Wrapper around a result of a domain action,
    /// containing a specific value.
    /// </summary>
    /// <typeparam name="T">Type of value to retrieve.</typeparam>
    public class Result<T> : Result
    {

        #region Properties

        /// <summary>
        /// Value holded by the result
        /// </summary>
        public T Value { get; private set; }

        #endregion

        #region Ctor
        /// <summary>
        /// Creates a new result with specific success flag
        /// and specific result value.
        /// </summary>
        /// <param name="isSuccess">Success flag</param>
        /// <param name="value">Result value.</param>
        protected Result(bool isSuccess, T value)
            : base(isSuccess)
        {
            Value = value;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Combine multiple result with current result.
        /// Final result contains enumeration of all results that have been generated.
        /// </summary>
        /// <param name="results"></param>
        /// <returns>All result values (including failed ones)</returns>
        public Result<IEnumerable<T>> Combine(params Result<T>[] results)
        {
            if (results == null)
            {
                return new Result<IEnumerable<T>>(IsSuccess, new[] { Value });
            }
            bool isSuccess = true;
            List<T> values = new List<T> { Value };
            foreach (var item in results)
            {
                if (!item.IsSuccess)
                {
                    isSuccess = false;
                }
                values.Add(item.Value);
            }
            var exportedValues = values.AsEnumerable();
            if (isSuccess)
            {
                return Result<IEnumerable<T>>.Ok(exportedValues);
            }
            else
            {
                return Result<IEnumerable<T>>.Fail(exportedValues);
            }
        }

        #endregion

        #region Public static methods

        /// <summary>
        /// Get a new failure result with a failure value.
        /// </summary>
        /// <param name="value">Specific failure value.</param>
        /// <returns>Instance of failure result that holds value.</returns>
        public static Result<T> Fail(T value) => new Result<T>(false, value);

        /// <summary>
        /// Get a new success result with a succes value.
        /// </summary>
        /// <param name="value">Specific success value.</param>
        /// <returns>Instance of success result that holds success value.</returns>
        public static Result<T> Ok(T value) => new Result<T>(true, value);

        #endregion

    }
}
