using RateLimiter;
using System;
using System.Threading.Tasks;

namespace RateLimiterTest
{
    public class MockTime : ITime
    {
        public DateTime Now { get; private set; }
        public int GetNowCount { get; private set; }
        public int GetDelayCount { get; private set; }
        public MockTime(DateTime now)
        {
            Now = now;
        }

        public void AddTime(TimeSpan addedTime)
        {
            Now = Now.Add(addedTime);
        }

        public Task GetDelay(TimeSpan timespan)
        {
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
