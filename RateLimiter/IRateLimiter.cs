using System;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter
{
    /// <summary>
    /// Rate limiter interface
    /// </summary>
    public interface IRateLimiter
    {
        /// <summary>
        /// Perform the given task respecting the time constraint
        /// </summary>
        /// <param name="perform"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Perform(Func<Task> perform, CancellationToken cancellationToken);

        /// <summary>
        /// Perform the given task respecting the time constraint
        /// </summary>
        /// <param name="perform"></param>
        /// <returns></returns>
        Task Perform(Func<Task> perform);

        /// <summary>
        /// Perform the given task respecting the time constraint
        /// returning the result of given function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="perform"></param>
        /// <returns></returns>
        Task<T> Perform<T>(Func<Task<T>> perform);

        /// <summary>
        /// Perform the given task respecting the time constraint
        /// returning the result of given function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="perform"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<T> Perform<T>(Func<Task<T>> perform, CancellationToken cancellationToken);

        /// <summary>
        /// Perform the given action respecting the time constraint
        /// </summary>
        /// <param name="perform"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Perform(Action perform, CancellationToken cancellationToken);

        /// <summary>
        /// Perform the given action respecting the time constraint
        /// </summary>
        /// <param name="perform"></param>
        /// <returns></returns>
        Task Perform(Action perform);

        /// <summary>
        /// Perform the given task respecting the time constraint
        /// returning the result of given function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="perform"></param>
        /// <returns></returns>
        Task<T> Perform<T>(Func<T> perform);

        /// <summary>
        /// Perform the given task respecting the time constraint
        /// returning the result of given function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="perform"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<T> Perform<T>(Func<T> perform, CancellationToken cancellationToken);
    }
}
