using System;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter.Tests
{
    public class MockTime : ITime
    {
        public DateTime Now { get; private set; }
        public int GetNowCount { get; private set; }
        public int GetDelayCount { get; private set; }
        public Action<TimeSpan, CancellationToken> OnDelay { get;  set; }

        public MockTime(DateTime now)
        {
            Now = now;
            OnDelay = (t, c) => { };
        }

        public void AddTime(TimeSpan addedTime)
        {
            Now = Now.Add(addedTime);
        }

        public Task GetDelay(TimeSpan timespan, CancellationToken cancellationToken)
        {
            OnDelay(timespan,  cancellationToken);
            GetDelayCount++;
            AddTime(timespan);
            return Task.FromResult(0);
        }

        public DateTime GetNow()
        {
            GetNowCount++;
            return Now;
        }
    }
}
