using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RateLimiter.Tests
{
    public class Sample
    {
        private readonly ITestOutputHelper _Output;

        public Sample(ITestOutputHelper output)
        {
            _Output = output;
        }

        private void ConsoleIt()
        {
            _Output.WriteLine($"{DateTime.Now:MM/dd/yyy HH:mm:ss.fff}");
        }

        [Fact(Skip = "for demo purpose only")]
        public async Task SimpleUsage()
        {
            var timeConstraint = TimeLimiter.GetFromMaxCountByInterval(5, TimeSpan.FromSeconds(1));

            for (int i = 0; i < 1000; i++)
            {
                await timeConstraint.Perform(() => ConsoleIt());
            }
        }

        [Fact]
        public async Task SimpleUsageWithCancellation()
        {
            var timeConstraint = TimeLimiter.GetFromMaxCountByInterval(5, TimeSpan.FromSeconds(1));
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(10));

            for (var i = 0; i < 1000; i++)
            {
                try
                {
                    await timeConstraint.Perform(() => ConsoleIt(), cts.Token);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        [Fact(Skip = "for demo purpose only")]
        public async Task TestOneThread()
        {
            var constraint = new CountByIntervalAwaitableConstraint(5, TimeSpan.FromSeconds(1));
            var constraint2 = new CountByIntervalAwaitableConstraint(1, TimeSpan.FromMilliseconds(100));
            var timeConstraint = TimeLimiter.Compose(constraint, constraint2);

            for (var i = 0; i < 1000; i++)
            {
                await timeConstraint.Perform(() => ConsoleIt());
            }
        }

        [Fact(Skip = "for demo purpose only")]
        public async Task Test100Thread()
        {
            var constraint = new CountByIntervalAwaitableConstraint(5, TimeSpan.FromSeconds(1));
            var constraint2 = new CountByIntervalAwaitableConstraint(1, TimeSpan.FromMilliseconds(100));
            var timeConstraint = TimeLimiter.Compose(constraint, constraint2);

            var tasks = new List<Task>();

            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    for (int j = 0; j < 10; j++)
                    {
                        await timeConstraint.Perform(() => ConsoleIt());
                    }
                }));
            }

            await Task.WhenAll(tasks.ToArray());
        }
    }
}
