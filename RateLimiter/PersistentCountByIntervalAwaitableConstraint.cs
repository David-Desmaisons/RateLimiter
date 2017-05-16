using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RateLimiter
{
    /// <summary>
    /// <see cref="CountByIntervalAwaitableConstraint"/> that is able to save own state.
    /// </summary>
    public class PersistentCountByIntervalAwaitableConstraint : CountByIntervalAwaitableConstraint
    {
        private readonly Action<DateTime> _saveStateAction;

        /// <summary>
        /// Create an instance of <see cref="PersistentCountByIntervalAwaitableConstraint"/>.
        /// </summary>
        /// <param name="count">Maximum actions allowed per time interval.</param>
        /// <param name="timeSpan">Time interval limits are applied for.</param>
        /// <param name="saveStateAction">Action is used to save state.</param>
        /// <param name="initialTimeStamps">Initial timestamps.</param>
        public PersistentCountByIntervalAwaitableConstraint(int count, TimeSpan timeSpan,
            Action<DateTime> saveStateAction, IEnumerable<DateTime> initialTimeStamps) : base(count, timeSpan)
        {
            _saveStateAction = saveStateAction;

            if (initialTimeStamps == null)
                return;

            foreach (var timeStamp in initialTimeStamps)
            {
                _TimeStamps.Push(timeStamp);
            }
        }

        /// <summary>
        /// Add new timestamp, save state, and release semaphore for next iterations.
        /// </summary>
        protected override void OnEnded()
        {
            var now = _Time.GetNow();
            _TimeStamps.Push(now);
            _saveStateAction(now);
            _Semafore.Release();
        }
    }
}