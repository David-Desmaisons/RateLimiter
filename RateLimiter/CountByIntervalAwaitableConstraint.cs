using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter
{
    public class CountByIntervalAwaitableConstraint : IAwaitableConstraint
    {
        public IReadOnlyList<DateTime> TimeStamps => _TimeStamps.ToList();

        protected int _Count { get; }
        protected TimeSpan _TimeSpan { get; }
        protected LimitedSizeStack<DateTime> _TimeStamps { get; }
        protected SemaphoreSlim _Semafore { get; } = new SemaphoreSlim(1, 1);
        protected ITime _Time { get; }

        public CountByIntervalAwaitableConstraint(int count, TimeSpan timeSpan)
        {
            if (count <= 0)
                throw new ArgumentException("count should be strictly positive", nameof(count));

            if (timeSpan.TotalMilliseconds <= 0)
                throw new ArgumentException("timeSpan should be strictly positive", nameof(timeSpan));

            _Count = count;
            _TimeSpan = timeSpan;
            _TimeStamps = new LimitedSizeStack<DateTime>(_Count);
            _Time = TimeSystem.StandardTime;
        }

        public virtual async Task<IDisposable> WaitForReadiness(CancellationToken cancellationToken)
        {
            await _Semafore.WaitAsync(cancellationToken);
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

            if (count < _Count)
                return new DisposeAction(OnEnded);

            Debug.Assert(element == null);
            Debug.Assert(last != null);
            var timetoWait = last.Value.Add(_TimeSpan) - now;
            try 
            {
                await _Time.GetDelay(timetoWait, cancellationToken);
            }
            catch (Exception) 
            {
                _Semafore.Release();
                throw;
            }           

            return new DisposeAction(OnEnded);
        }

        protected virtual void OnEnded() 
        {
            _TimeStamps.Push(_Time.GetNow());
            _Semafore.Release();
        }
    }
}
