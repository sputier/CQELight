using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.DDD
{
    /// <summary>
    /// Wrapper around a result of a domain action.
    /// </summary>
    public class Result
    {

        #region Properties

        /// <summary>
        /// Flag that indicates if result is success or failure.
        /// </summary>
        public bool IsSuccess { get; private set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Creates a new Result with a specific success flag.
        /// </summary>
        /// <param name="isSuccess">Succes flag value.</param>
        protected Result(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        #endregion

        #region Public static methods

        /// <summary>
        /// Get a standard failure result that doesn't holds any
        /// specific value.
        /// </summary>
        /// <returns>Failure result.</returns>
        public static Result Fail() => new Result(false);
        /// <summary>
        /// Get a standard success result.
        /// </summary>
        /// <returns>Success result.</returns>
        public static Result Ok() => new Result(true);
        /// <summary>
        /// Get a success result with a defined value.
        /// </summary>
        /// <typeparam name="T">Type of value to use in result.</typeparam>
        /// <param name="value">Value to use in result.</param>
        /// <returns>Succes result with value.</returns>
        public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);
        /// <summary>
        /// Get a failed result with a defined value.
        /// </summary>
        /// <typeparam name="T">Type of value to use in result.</typeparam>
        /// <param name="value">Value to use in result.</param>
        /// <returns>Failed result with value.</returns>
        public static Result<T> Fail<T>(T value) => Result<T>.Ok(value);

        #endregion

        #region Operator

        public static implicit operator bool(Result r)
            => r.IsSuccess;

        #endregion

    }

}
