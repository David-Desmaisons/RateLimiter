﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter
{
    /// <summary>
    /// <see cref="IRateLimiter"/> implementation
    /// </summary>
    public class TimeLimiter : IRateLimiter
    {
        internal readonly IAwaitableConstraint _AwaitableConstraint;

        internal TimeLimiter(IAwaitableConstraint awaitableConstraint)
        {
            _AwaitableConstraint = awaitableConstraint;
        }

        /// <summary>
        /// Perform the given task respecting the time constraint
        /// returning the result of given function
        /// </summary>
        /// <param name="perform"></param>
        /// <returns></returns>
        public Task Perform(Func<Task> perform) 
        {
            return Perform(perform, CancellationToken.None);
        }

        /// <summary>
        /// Perform the given task respecting the time constraint
        /// returning the result of given function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="perform"></param>
        /// <returns></returns>
        public Task<T> Perform<T>(Func<Task<T>> perform) 
        {
            return Perform(perform, CancellationToken.None);
        }

        /// <summary>
        /// Perform the given task respecting the time constraint
        /// </summary>
        /// <param name="perform"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Perform(Func<Task> perform, CancellationToken cancellationToken) 
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (await _AwaitableConstraint.WaitForReadiness(cancellationToken)) 
            {
                await perform();
            }
        }

        /// <summary>
        /// Perform the given task respecting the time constraint
        /// returning the result of given function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="perform"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<T> Perform<T>(Func<Task<T>> perform, CancellationToken cancellationToken) 
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (await _AwaitableConstraint.WaitForReadiness(cancellationToken)) 
            {
                return await perform();
            }
        }

        private static Func<Task> Transform(Action act) 
        {
            return () => { act(); return Task.FromResult(0); };
        }

        /// <summary>
        /// Perform the given task respecting the time constraint
        /// returning the result of given function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="compute"></param>
        /// <returns></returns>
        private static Func<Task<T>> Transform<T>(Func<T> compute) 
        {
            return () =>  Task.FromResult(compute()); 
        }

        /// <summary>
        /// Perform the given task respecting the time constraint
        /// </summary>
        /// <param name="perform"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Perform(Action perform, CancellationToken cancellationToken) 
        {
           var transformed = Transform(perform);
           return Perform(transformed, cancellationToken);
        }

        /// <summary>
        /// Perform the given task respecting the time constraint
        /// </summary>
        /// <param name="perform"></param>
        /// <returns></returns>
        public Task Perform(Action perform) 
        {
            var transformed = Transform(perform);
            return Perform(transformed);
        }

        /// <summary>
        /// Perform the given task respecting the time constraint
        /// returning the result of given function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="perform"></param>
        /// <returns></returns>
        public Task<T> Perform<T>(Func<T> perform) 
        {
            var transformed = Transform(perform);
            return Perform(transformed);
        }

        /// <summary>
        /// Perform the given task respecting the time constraint
        /// returning the result of given function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="perform"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<T> Perform<T>(Func<T> perform, CancellationToken cancellationToken) 
        {
            var transformed = Transform(perform);
            return Perform(transformed, cancellationToken);
        }

        /// <summary>
        /// Returns a TimeLimiter based on a maximum number of times
        /// during a given period
        /// </summary>
        /// <param name="maxCount"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static TimeLimiter GetFromMaxCountByInterval(int maxCount, TimeSpan timeSpan)
        {
            return new TimeLimiter(new CountByIntervalAwaitableConstraint(maxCount, timeSpan));
        }

        /// <summary>
        /// Create <see cref="TimeLimiter"/> that will save state using action passed through <paramref name="saveStateAction"/> parameter.
        /// </summary>
        /// <param name="maxCount">Maximum actions allowed per time interval.</param>
        /// <param name="timeSpan">Time interval limits are applied for.</param>
        /// <param name="saveStateAction">Action is used to save state.</param>
        /// <returns><see cref="TimeLimiter"/> instance with <see cref="PersistentCountByIntervalAwaitableConstraint"/>.</returns>
        public static TimeLimiter GetPersistentTimeLimiter(int maxCount, TimeSpan timeSpan,
            Action<DateTime> saveStateAction)
        {
            return GetPersistentTimeLimiter(maxCount, timeSpan, saveStateAction, null);
        }

        /// <summary>
        /// Create <see cref="TimeLimiter"/> with initial timestamps that will save state using action passed through <paramref name="saveStateAction"/> parameter.
        /// </summary>
        /// <param name="maxCount">Maximum actions allowed per time interval.</param>
        /// <param name="timeSpan">Time interval limits are applied for.</param>
        /// <param name="saveStateAction">Action is used to save state.</param>
        /// <param name="initialTimeStamps">Initial timestamps.</param>
        /// <returns><see cref="TimeLimiter"/> instance with <see cref="PersistentCountByIntervalAwaitableConstraint"/>.</returns>
        public static TimeLimiter GetPersistentTimeLimiter(int maxCount, TimeSpan timeSpan,
            Action<DateTime> saveStateAction, IEnumerable<DateTime> initialTimeStamps)
        {
            return new TimeLimiter(new PersistentCountByIntervalAwaitableConstraint(maxCount, timeSpan, saveStateAction, initialTimeStamps));
        }

        /// <summary>
        /// Compose various IAwaitableConstraint in a TimeLimiter
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        public static TimeLimiter Compose(params IAwaitableConstraint[] constraints)
        {
            var composed = constraints.Aggregate(default(IAwaitableConstraint), 
                (accumulated, current) => (accumulated == null) ? current : accumulated.Compose(current));
            return new TimeLimiter(composed);
        }
    }
}
