using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using ComposableAsync;
using FluentAssertions;

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
                await timeConstraint.Enqueue(() => ConsoleIt());
            }
        }

        [Fact]
        public async Task SimpleUsageWithCancellation()
        {
            var timeConstraint = TimeLimiter.GetFromMaxCountByInterval(5, TimeSpan.FromSeconds(1));
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            for (var i = 0; i < 1000; i++)
            {
                try
                {
                    await timeConstraint.Enqueue(() => ConsoleIt(), cts.Token);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        [Fact]
        public async Task SimpleUsageAwaitable()
        {
            var timeConstraint = TimeLimiter.GetFromMaxCountByInterval(5, TimeSpan.FromSeconds(1));
            for (var i = 0; i < 50; i++)
            {
                await timeConstraint;
                ConsoleIt();
            }
        }

        [Fact]
        public async Task SimpleUsageAwaitableCancellable()
        {
            var timeConstraint = TimeLimiter.GetFromMaxCountByInterval(5, TimeSpan.FromSeconds(1));
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1.1));
            var count = new Count();

            Func<Task> cancellable = () => DoUntil(timeConstraint, cts.Token, count);

            await cancellable.Should().ThrowAsync<OperationCanceledException>();
            count.Value.Should().Be(10);
        }

        public class Count
        {
            public int Value { get; set; }
        }

        private async Task DoUntil(ICancellableDispatcher timeConstraint, CancellationToken token, Count count)
        {
            while (true)
            {
                await timeConstraint;
                token.ThrowIfCancellationRequested();
                ConsoleIt();
                count.Value++;
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
                await timeConstraint.Enqueue(() => ConsoleIt());
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
                        await timeConstraint.Enqueue(() => ConsoleIt());
                    }
                }));
            }

            await Task.WhenAll(tasks.ToArray());
        }
    }
}
