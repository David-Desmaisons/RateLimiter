﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter
{
    /// <summary>
    /// Provide an awaitable constraint based on number of times per duration
    /// </summary>
    public class CallCountByIntervalAwaitableConstraint : IAwaitableConstraint
    {
        /// <summary>
        /// List of the last time stamps
        /// </summary>
        public IReadOnlyList<DateTime> TimeStamps => _TimeStamps.ToList();

        /// <summary>
        /// Stack of the last time stamps
        /// </summary>
        protected LimitedSizeStack<DateTime> _TimeStamps { get; }

        private int _Count { get; }
        private TimeSpan _TimeSpan { get; }
        private SemaphoreSlim _Semaphore { get; } = new SemaphoreSlim(1, 1);
        private ITime _Time { get; }

        /// <summary>
        /// Constructs a new AwaitableConstraint based on number of times per duration
        /// </summary>
        /// <param name="count"></param>
        /// <param name="timeSpan"></param>
        public CallCountByIntervalAwaitableConstraint(int count, TimeSpan timeSpan) : this(count, timeSpan, TimeSystem.StandardTime)
        {
        }

        internal CallCountByIntervalAwaitableConstraint(int count, TimeSpan timeSpan, ITime time)
        {
            if (count <= 0)
                throw new ArgumentException("count should be strictly positive", nameof(count));

            if (timeSpan.TotalMilliseconds <= 0)
                throw new ArgumentException("timeSpan should be strictly positive", nameof(timeSpan));

            _Count = count;
            _TimeSpan = timeSpan;
            _TimeStamps = new LimitedSizeStack<DateTime>(_Count);
            _Time = time;
        }

        /// <summary>
        /// returns a task that will complete once the constraint is fulfilled
        /// </summary>
        /// <param name="cancellationToken">
        /// Cancel the wait
        /// </param>
        /// <returns>
        /// A disposable that should be disposed upon task completion
        /// </returns>
        public async Task<IDisposable> WaitForReadiness(CancellationToken cancellationToken)
        {
            try
            {

                await _Semaphore.WaitAsync(cancellationToken);
                var count = 0;
                var now = _Time.GetNow();
                var target = now - _TimeSpan;
                LinkedListNode<DateTime> element = _TimeStamps.First, last = null;
                while ((element != null) && (element.Value > target))
                {
                    last = element;
                    element = element.Next;
                    count++;
                }

                if (count >= _Count)
                {
                    var timeToWait = last.Value.Add(_TimeSpan) - now;
                    now = now.Add(timeToWait);
                    Debug.Assert(element == null);
                    Debug.Assert(last != null);
                    await _Time.GetDelay(timeToWait, cancellationToken);
                }
                _TimeStamps.Push(now);
                _Semaphore.Release();

                return new DisposeAction(OnEnded);
            }
            catch (Exception)
            {
                _Semaphore.Release();
                throw;
            }
        }

        /// <summary>
        /// Clone CallCountByIntervalAwaitableConstraint
        /// </summary>
        /// <returns></returns>
        public IAwaitableConstraint Clone()
        {
            return new CallCountByIntervalAwaitableConstraint(_Count, _TimeSpan, _Time);
        }

        private void OnEnded()
        {
            OnEnded(TimeSystem.StandardTime.GetNow());
        }

        /// <summary>
        /// Called when action has been executed
        /// </summary>
        /// <param name="now"></param>
        protected virtual void OnEnded(DateTime now)
        {
        }
    }
}
