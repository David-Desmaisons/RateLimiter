using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using ComposableAsync;
using FluentAssertions;
using RateLimiter.Tests.TestClass;
using ComposableAsync.Factory;

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
            var token = cts.Token;
            var count = 0;

            Func<Task> cancellable = async () => {
                while (true) {
                    await timeConstraint;
                    token.ThrowIfCancellationRequested();
                    ConsoleIt();
                    count++;
                }
            };

            await cancellable.Should().ThrowAsync<OperationCanceledException>();
            count.Should().Be(10);
        }
       
        [Fact]
        public async Task UsageWithFactory()
        {
            var timeConstraint = TimeLimiter.GetFromMaxCountByInterval(5, TimeSpan.FromMilliseconds(100));
            var proxyFactory = new ProxyFactory(timeConstraint);
            var wrapped = new TimeLimited(_Output);
            var timeLimited = proxyFactory.Build<ITimeLimited>(wrapped);

            for (var i = 0; i < 50; i++)
            {
                await timeLimited.GetValue();
            }

            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(110));

            Func<Task> cancellable = async () =>
            {
                while (true) 
                {
                    await timeLimited.GetValue(cts.Token);
                }
            };

            await cancellable.Should().ThrowAsync<OperationCanceledException>();
             
            var res = await timeLimited.GetValue();
            res.Should().Be(56);
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
