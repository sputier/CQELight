using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.IoC;
using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.CQS
{
    /// <summary>
    /// Helping class to execute query easily
    /// </summary>
    public static class QueryExecuter
    {

        #region Members

        private static IScope _scope;

        #endregion

        #region Ctor

        static QueryExecuter()
        {
            if (DIManager.IsInit)
            {
                _scope = DIManager.BeginScope();
            }
        }

        #endregion

        #region Public static methods

        /// <summary>
        /// Executes a query with no parameters.
        /// </summary>
        /// <typeparam name="TQuery">Type of query to execute.</typeparam>
        /// <typeparam name="TResult">Type of result to retrieve.</typeparam>
        /// <returns>A task to wait for the result.</returns>
        public static Task<TResult> ExecuteQueryAsync<TQuery, TResult>()
            where TQuery : class, IQuery<TResult>
        {
            var q = GetQuery<TQuery>();
            if (q != null)
            {
                return q.ExecuteQueryAsync();
            }
            return Task.FromResult<TResult>(default);
        }

        /// <summary>
        /// Executes a query with one parameter.
        /// </summary>
        /// <param name="param">Value of the parameter.</param>
        /// <typeparam name="TQuery">Type of query to execute.</typeparam>
        /// <typeparam name="TResult">Type of result to retrieve.</typeparam>
        /// <typeparam name="TParam">Type of the first param.</typeparam>
        /// <returns>A task to wait for the result.</returns>
        public static Task<TResult> ExecuteQueryAsync<TQuery, TResult, TParam>(TParam param)
            where TQuery : class, IQuery<TResult, TParam>
        {
            var q = GetQuery<TQuery>();

            if (q != null)
            {
                return q.ExecuteQueryAsync(param);
            }
            return Task.FromResult<TResult>(default);
        }

        /// <summary>
        /// Executes a query with two parameters.
        /// </summary>
        /// <param name="param">Value of the first parameter.</param>
        /// <param name="param2">Value of the second parameter.</param>
        /// <typeparam name="TQuery">Type of query to execute.</typeparam>
        /// <typeparam name="TResult">Type of result to retrieve.</typeparam>
        /// <typeparam name="TParam">Type of the first param.</typeparam>
        /// <typeparam name="TParam2">Type of the second param.</typeparam>
        /// <returns>A task to wait for the result.</returns>
        public static Task<TResult> ExecuteQueryAsync<TQuery, TResult, TParam, TParam2>(TParam param, TParam2 param2)
            where TQuery : class, IQuery<TResult, TParam, TParam2>
        {
            var q = GetQuery<TQuery>();

            if (q != null)
            {
                return q.ExecuteQueryAsync(param, param2);
            }
            return Task.FromResult<TResult>(default);
        }

        /// <summary>
        /// Executes a query with three parameters.
        /// </summary>
        /// <param name="param">Value of the first parameter.</param>
        /// <param name="param2">Value of the second parameter.</param>
        /// <param name="param3">Value of the third parameter.</param>
        /// <typeparam name="TQuery">Type of query to execute.</typeparam>
        /// <typeparam name="TResult">Type of result to retrieve.</typeparam>
        /// <typeparam name="TParam">Type of the first param.</typeparam>
        /// <typeparam name="TParam2">Type of the second param.</typeparam>
        /// <typeparam name="TParam3">Type of the third param.</typeparam>
        /// <returns>A task to wait for the result.</returns>
        public static Task<TResult> ExecuteQueryAsync<TQuery, TResult, TParam, TParam2, TParam3>(TParam param, TParam2 param2, TParam3 param3)
            where TQuery : class, IQuery<TResult, TParam, TParam2, TParam3>
        {
            var q = GetQuery<TQuery>();

            if (q != null)
            {
                return q.ExecuteQueryAsync(param, param2, param3);
            }
            return Task.FromResult<TResult>(default);
        }

        /// <summary>
        /// Executes a query with four parameters.
        /// </summary>
        /// <param name="param">Value of the first parameter.</param>
        /// <param name="param2">Value of the second parameter.</param>
        /// <param name="param3">Value of the third parameter.</param>
        /// <param name="param4">Value of the fourth parameter.</param>
        /// <typeparam name="TQuery">Type of query to execute.</typeparam>
        /// <typeparam name="TResult">Type of result to retrieve.</typeparam>
        /// <typeparam name="TParam">Type of the first param.</typeparam>
        /// <typeparam name="TParam2">Type of the second param.</typeparam>
        /// <typeparam name="TParam3">Type of the third param.</typeparam>
        /// <typeparam name="TParam4">Type of the fourth param.</typeparam>
        /// <returns>A task to wait for the result.</returns>
        public static Task<TResult> ExecuteQueryAsync<TQuery, TResult, TParam, TParam2, TParam3, TParam4>(TParam param, TParam2 param2,
            TParam3 param3, TParam4 param4)
            where TQuery : class, IQuery<TResult, TParam, TParam2, TParam3, TParam4>
        {
            var q = GetQuery<TQuery>();

            if (q != null)
            {
                return q.ExecuteQueryAsync(param, param2, param3, param4);
            }
            return Task.FromResult<TResult>(default);
        }

        /// <summary>
        /// Executes a query with five parameters.
        /// </summary>
        /// <param name="param">Value of the first parameter.</param>
        /// <param name="param2">Value of the second parameter.</param>
        /// <param name="param3">Value of the third parameter.</param>
        /// <param name="param4">Value of the fourth parameter.</param>
        /// <param name="param5">Value of the fifth parameter.</param>
        /// <typeparam name="TQuery">Type of query to execute.</typeparam>
        /// <typeparam name="TResult">Type of result to retrieve.</typeparam>
        /// <typeparam name="TParam">Type of the first param.</typeparam>
        /// <typeparam name="TParam2">Type of the second param.</typeparam>
        /// <typeparam name="TParam3">Type of the third param.</typeparam>
        /// <typeparam name="TParam4">Type of the fourth param.</typeparam>
        /// <typeparam name="TParam5">Type of the fifth param.</typeparam>
        /// <returns>A task to wait for the result.</returns>
        public static Task<TResult> ExecuteQueryAsync<TQuery, TResult, TParam, TParam2, TParam3, TParam4, TParam5>(TParam param, TParam2 param2,
            TParam3 param3, TParam4 param4, TParam5 param5)
            where TQuery : class, IQuery<TResult, TParam, TParam2, TParam3, TParam4, TParam5>
        {
            var q = GetQuery<TQuery>();

            if (q != null)
            {
                return q.ExecuteQueryAsync(param, param2, param3, param4, param5);
            }
            return Task.FromResult<TResult>(default);
        }

        #endregion

        #region Private methods

        private static TQuery GetQuery<TQuery>()
            where TQuery : class
            => _scope != null
                ? _scope.Resolve<TQuery>()
                : typeof(TQuery).CreateInstance<TQuery>();

        #endregion

    }
}
