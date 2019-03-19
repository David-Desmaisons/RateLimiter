using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter
{
    /// <summary>
    /// Provides an awaiter for awaiting a <see cref="TimeLimiter"/>.
    /// </summary>
    /// <remarks>This type is intended for compiler use only.</remarks>
    public struct TimeLimiterAwaiter : INotifyCompletion
    {
        Task<IDisposable> _task;

        internal TimeLimiterAwaiter(TimeLimiter timeLimiter)
        {
            _task = timeLimiter._AwaitableConstraint.WaitForReadiness(CancellationToken.None);
        }

        /// <summary>
        /// Ends the await on the completed <see cref="TimeLimiter"/>.
        /// </summary>
        public void GetResult()
        {
            _task.Result.Dispose();
        }

        /// <summary>
        /// Gets whether the <see cref="TimeLimiter"/> being awaited is ready.
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                return _task.IsCompleted;
            }
        }

        /// <summary>
        /// Schedules the continuation.
        /// </summary>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        public void OnCompleted(Action continuation)
        {
            new Task(continuation).Start();
        }
    }
}
