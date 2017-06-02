using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Abstractions
{
    /// <summary>
    /// Contract interface for simple query without params.
    /// </summary>
    /// <typeparam name="TOut">Element type to retrieve.</typeparam>
    public interface IQuery<TOut>
    {
        /// <summary>
        /// Executes query asynchronously.
        /// </summary>
        /// <returns>Found instance of TOut</returns>
        Task<TOut> ExecuteQueryAsync();
    }
    /// <summary>
    /// Contract interface for simple query with one param.
    /// </summary>
    /// <typeparam name="TOut">Element type to retrieve.</typeparam>
    /// <typeparam name="TParam">Type of first param.</typeparam>
    public interface IQuery<TOut, TParam>
    {
        /// <summary>
        /// Executes query asynchronously.
        /// </summary>
        /// <param name="param">First param.</param>
        /// <returns>Found instance of TOut</returns>
        Task<TOut> ExecuteQueryAsync(TParam param);
    }
    /// <summary>
    /// Contract interface for simple query with one param.
    /// </summary>
    /// <typeparam name="TOut">Element type to retrieve.</typeparam>
    /// <typeparam name="TParam">Type of first param.</typeparam>
    /// <typeparam name="TParam2">Type of second param.</typeparam>
    public interface IQuery<TOut, TParam, TParam2>
    {
        /// <summary>
        /// Executes query asynchronously.
        /// </summary>
        /// <param name="param">First parameter.</param>
        /// <param name="param2">Second parameter.</param>
        /// <returns>Found instance of TOut</returns>
        Task<TOut> ExecuteQueryAsync(TParam param, TParam2 param2);
    }
    /// <summary>
    /// Contract interface for simple query with one param.
    /// </summary>
    /// <typeparam name="TOut">Element type to retrieve.</typeparam>
    /// <typeparam name="TParam">Type of first param.</typeparam>
    /// <typeparam name="TParam2">Type of second param.</typeparam>
    /// <typeparam name="TParam3">Type of third param.</typeparam>
    public interface IQuery<TOut, TParam, TParam2, TParam3>
    {
        /// <summary>
        /// Executes query asynchronously.
        /// </summary>
        /// <param name="param">First parameter.</param>
        /// <param name="param2">Second parameter.</param>
        /// <param name="param3">Third parameter.</param>
        /// <returns>Found instance of TOut</returns>
        Task<TOut> ExecuteQueryAsync(TParam param, TParam2 param2, TParam3 param3);
    }
    /// <summary>
    /// Contract interface for simple query with one param.
    /// </summary>
    /// <typeparam name="TOut">Element type to retrieve.</typeparam>
    /// <typeparam name="TParam">Type of first param.</typeparam>
    /// <typeparam name="TParam2">Type of second param.</typeparam>
    /// <typeparam name="TParam3">Type of third param.</typeparam>
    /// <typeparam name="TParam4">Type of 4th param.</typeparam>
    public interface IQuery<TOut, TParam, TParam2, TParam3, TParam4>
    {
        /// <summary>
        /// Executes query asynchronously.
        /// </summary>
        /// <param name="param">First parameter.</param>
        /// <param name="param2">Second parameter.</param>
        /// <param name="param3">Third parameter.</param>
        /// <param name="param4">4th parameter.</param>
        /// <returns>Found instance of TOut</returns>
        Task<TOut> ExecuteQueryAsync(TParam param, TParam2 param2, TParam3 param3, TParam4 param4);
    }
    /// <summary>
    /// Contract interface for simple query with one param.
    /// </summary>
    /// <typeparam name="TOut">Element type to retrieve.</typeparam>
    /// <typeparam name="TParam">Type of first param.</typeparam>
    /// <typeparam name="TParam2">Type of second param.</typeparam>
    /// <typeparam name="TParam3">Type of third param.</typeparam>
    /// <typeparam name="TParam4">Type of 4th param.</typeparam>
    /// <typeparam name="TParam5">Type of 5th param.</typeparam>
    public interface IQuery<TOut, TParam, TParam2, TParam3, TParam4, TParam5>
    {
        /// <summary>
        /// Executes query asynchronously.
        /// </summary>
        /// <param name="param">First parameter.</param>
        /// <param name="param2">Second parameter.</param>
        /// <param name="param3">Third parameter.</param>
        /// <param name="param4">4th parameter.</param>
        /// <param name="param5">5th parameter.</param>
        /// <returns>Found instance of TOut</returns>
        Task<TOut> ExecuteQueryAsync(TParam param, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5);
    }
}
