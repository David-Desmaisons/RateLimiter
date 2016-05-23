using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter
{
    public class CountByIntervalAwaitableConstraint : IAwaitableConstraint
    {
        private readonly int _Count;
        private readonly TimeSpan _TimeSpan;
        private readonly LimitedSizeStack<DateTime> _TimeStamps;
        private readonly SemaphoreSlim _Semafore = new SemaphoreSlim(1, 1);
        private readonly ITime _Time;

        public CountByIntervalAwaitableConstraint(int count, TimeSpan timeSpan)
        {
            if (count <= 0)
                throw new ArgumentException("count should be strictly positive", "count");

            if (timeSpan.TotalMilliseconds <= 0)
                throw new ArgumentException("timeSpan should be strictly positive", "timeSpan");

            _Count = count;
            _TimeSpan = timeSpan;
            _TimeStamps = new LimitedSizeStack<DateTime>(_Count);
            _Time = TimeSystem.StandardTime;
        }

        public async Task WaitForReadiness()
        {
            await _Semafore.WaitAsync();
            var count = 0;
            var now = _Time.GetNow();
            var target = now - _TimeSpan;
            LinkedListNode<DateTime> Element = _TimeStamps.First, last = null;
            while ((Element != null) && (Element.Value > target))
            {
                last = Element;
                Element = Element.Next;
                count++;
            }

            if (count < _Count)
                return;

            Debug.Assert(Element == null);
            Debug.Assert(last != null);
            var timetoWait = last.Value.Add(_TimeSpan) - now;
            await _Time.GetDelay(timetoWait);
        }

        public void Execute()
        {
            _TimeStamps.Push(_Time.GetNow());
            _Semafore.Release();
        }
    }
}
