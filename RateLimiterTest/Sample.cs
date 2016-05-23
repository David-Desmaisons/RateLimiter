using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using RateLimiter;
using Xunit;

namespace RateLimiterTest
{
    public class Sample
    {

        private Task ConsoleIt()
        {
            Trace.WriteLine(string.Format("{0:MM/dd/yyy HH:mm:ss.fff}", DateTime.Now));
            return Task.FromResult(0);
        }

        [Fact(Skip = "for demo purpose only")]
        public async Task TestOneThread()
        {
            var constraint = new CountByIntervalAwaitableConstraint(5, TimeSpan.FromSeconds(1));
            var constraint2 = new CountByIntervalAwaitableConstraint(1, TimeSpan.FromMilliseconds(100));
            var timeconstraint = new TimeLimiter(constraint.Compose(constraint2));

            for(int i=0; i<1000; i++)
            {
                await timeconstraint.Perform(ConsoleIt);
            }       
        }

        [Fact(Skip = "for demo purpose only")]
        public async Task Test100Thread()
        {
            var constraint = new CountByIntervalAwaitableConstraint(5, TimeSpan.FromSeconds(1));
            var constraint2 = new CountByIntervalAwaitableConstraint(1, TimeSpan.FromMilliseconds(100));
            var timeconstraint = new TimeLimiter(constraint.Compose(constraint2));

            var tasks = new List<Task>();

            for (int i = 0; i < 100; i++)
            {
                tasks.Add( Task.Run(async () =>
                 {
                     for (int j = 0; j < 10; j++)
                     {
                         await timeconstraint.Perform(ConsoleIt);
                     }
                 }));          
            }

            await Task.WhenAll(tasks.ToArray());
        }
    }
}
