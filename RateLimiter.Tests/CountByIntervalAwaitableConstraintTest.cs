using FluentAssertions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RateLimiter.Tests
{
    public class CountByIntervalAwaitableConstraintTest
    {
        private readonly CountByIntervalAwaitableConstraint _CountByIntervalAwaitableConstraint1;
        private readonly CountByIntervalAwaitableConstraint _CountByIntervalAwaitableConstraint2;
        private readonly MockTime _MockTime;
        private readonly DateTime _Origin;

        public CountByIntervalAwaitableConstraintTest()
        {
            _Origin = new DateTime(2000, 1, 1);
            _MockTime = new MockTime(_Origin);
            _CountByIntervalAwaitableConstraint1 = new CountByIntervalAwaitableConstraint(1, TimeSpan.FromMilliseconds(100), _MockTime);
            _CountByIntervalAwaitableConstraint2 = new CountByIntervalAwaitableConstraint(5, TimeSpan.FromSeconds(1), _MockTime);
        }

        [Fact]
        public void Constructor_WithNegativeCount_ThrowException()
        {
            Action act = () => new CountByIntervalAwaitableConstraint(-1, TimeSpan.FromSeconds(1));
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor_WithNegativeTimeSpan_ThrowException()
        {
            Action act = () => new CountByIntervalAwaitableConstraint(10, TimeSpan.FromSeconds(-1));
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public async Task WaitForReadinessIsSynchronous_FirstTime()
        {
            var task = _CountByIntervalAwaitableConstraint1.WaitForReadiness(CancellationToken.None);
            var timedOut = await TaskHasTimeOut(task, 10);
            timedOut.Should().BeFalse();
        }

        [Fact]
        public async Task WaitForReadinessDoNotCallDelay_FirstTime()
        {
            await _CountByIntervalAwaitableConstraint1.WaitForReadiness(CancellationToken.None);
            _MockTime.GetDelayCount.Should().Be(0);
        }

        [Fact]
        public async Task WaitForReadiness_Block_WhenWaitForReadinessHasBeenCalledButNotExecute()
        {
            await _CountByIntervalAwaitableConstraint1.WaitForReadiness(CancellationToken.None);

            var secondTask = _CountByIntervalAwaitableConstraint1.WaitForReadiness(CancellationToken.None);
            var timedOut = await TaskHasTimeOut(secondTask, 300);
            timedOut.Should().BeTrue();
        }

        [Fact]
        public async Task WaitForReadiness_UnBlock_WhenDisposeIsCalled()
        {
            var disp = await _CountByIntervalAwaitableConstraint1.WaitForReadiness(CancellationToken.None);
            var secondTask = _CountByIntervalAwaitableConstraint1.WaitForReadiness(CancellationToken.None);
            await TaskHasTimeOut(secondTask, 10);
            disp.Dispose();

            var timedOut = await TaskHasTimeOut(secondTask, 300);
            timedOut.Should().BeFalse();
        }

        [Fact]
        public async Task WaitForReadiness_WhenCancelled_DoNotBlock()
        {
            var cancellation = new CancellationToken(true);
            try
            {
                await _CountByIntervalAwaitableConstraint1.WaitForReadiness(cancellation);
            }
            catch
            {
            }
            var task = _CountByIntervalAwaitableConstraint1.WaitForReadiness(CancellationToken.None);
            var timedOut = await TaskHasTimeOut(task, 50);
            timedOut.Should().BeFalse();
        }

        [Fact]
        public void WaitForReadiness_WhenCancelled_ThrowException()
        {
            var cancellation = new CancellationToken(true);
            Func<Task> act = async () => await _CountByIntervalAwaitableConstraint1.WaitForReadiness(cancellation);
            act.Should().Throw<TaskCanceledException>();
        }

        [Fact]
        public async Task WaitForReadiness_WhenCancelledAfterSemaphoreTaken_DoNotBlock()
        {
            var cancellation = await SetUpForCancelledAfterSemaphoreTaken();
            try
            {
                await _CountByIntervalAwaitableConstraint1.WaitForReadiness(cancellation);
            }
            catch
            {
            }
            var task = _CountByIntervalAwaitableConstraint1.WaitForReadiness(CancellationToken.None);
            var timedOut = await TaskHasTimeOut(task, 50);
            timedOut.Should().BeFalse();
        }

        [Fact]
        public async Task WaitForReadiness_WhenCancelledAfterSemaphoreTaken_ThrowException()
        {
            var cancellation = await SetUpForCancelledAfterSemaphoreTaken();
            Func<Task> act = async () => await _CountByIntervalAwaitableConstraint1.WaitForReadiness(cancellation);
            act.Should().Throw<TaskCanceledException>();
        }

        private async Task<CancellationToken> SetUpForCancelledAfterSemaphoreTaken()
        {
            var cancellationSource = new CancellationTokenSource();
            _MockTime.OnDelay = (t, c) =>
            {
                cancellationSource.Cancel();
                cancellationSource.Token.ThrowIfCancellationRequested();
            };

            var disp = await _CountByIntervalAwaitableConstraint1.WaitForReadiness(CancellationToken.None);
            disp.Dispose();

            return cancellationSource.Token;
        }

        [Fact]
        public async Task WaitForReadiness_WhenLimitIsReached_CallDelayToRespectTimeConstraint()
        {
            await SetUpSaturated(_CountByIntervalAwaitableConstraint1);
            await CheckCompletionInTime();
        }

        [Fact]
        public async Task WaitForReadiness_WhenLimitIsReached_CallDelayToRespectTimeConstraintWithMinimalDelay()
        {
            await SetUpSaturated(_CountByIntervalAwaitableConstraint1);
            _MockTime.AddTime(TimeSpan.FromMilliseconds(50));
            await CheckCompletionInTime();
        }

        [Fact]
        public async Task WaitForReadiness_WhenLimitIsNotReached_DoNotCallDelayToRespectTimeConstraint()
        {
            await SetUpSaturated(_CountByIntervalAwaitableConstraint1);
            _MockTime.AddTime(TimeSpan.FromMilliseconds(100));
            await _CountByIntervalAwaitableConstraint1.WaitForReadiness(CancellationToken.None);
            _MockTime.GetDelayCount.Should().Be(0);
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(1, false)]
        [InlineData(3, false)]
        [InlineData(2, false)]
        [InlineData(4, false)]
        [InlineData(5, true)]
        [InlineData(6, true)]
        [InlineData(7, true)]
        public async Task WaitForReadiness_LimitIsBasedOnCount(int count, bool wait)
        {
            await Test_WaitForReadiness_LimitIsBasedOnCount(count, _CountByIntervalAwaitableConstraint2);

            await _CountByIntervalAwaitableConstraint2.WaitForReadiness(CancellationToken.None);
            _MockTime.GetDelayCount.Should().Be(wait ? 1 : 0);
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(1, false)]
        [InlineData(3, false)]
        [InlineData(2, false)]
        [InlineData(4, false)]
        [InlineData(5, true)]
        [InlineData(6, true)]
        [InlineData(7, true)]
        public async Task Clone_WaitForReadiness_LimitIsBasedOnCount(int count, bool wait)
        {
            await Test_WaitForReadiness_LimitIsBasedOnCount(4, _CountByIntervalAwaitableConstraint2);

            var cloned = _CountByIntervalAwaitableConstraint2.Clone();
            await Test_WaitForReadiness_LimitIsBasedOnCount(count, cloned);

            await cloned.WaitForReadiness(CancellationToken.None);
            _MockTime.GetDelayCount.Should().Be(wait ? 1 : 0);
        }

        private async Task Test_WaitForReadiness_LimitIsBasedOnCount(int count, IAwaitableConstraint countByIntervalAwaitableConstraint)
        {
            for (var i = 0; i < count; i++)
            {
                await SetUpSaturated(countByIntervalAwaitableConstraint);
            }
        }

        private static async Task SetUpSaturated(IAwaitableConstraint countByIntervalAwaitableConstraint)
        {
            using (await countByIntervalAwaitableConstraint.WaitForReadiness(CancellationToken.None))
            {
            }
        }

        private async Task CheckCompletionInTime()
        {
            await _CountByIntervalAwaitableConstraint1.WaitForReadiness(CancellationToken.None);
            _MockTime.GetDelayCount.Should().Be(1);
            _MockTime.Now.Should().Be(_Origin.AddMilliseconds(100));
        }

        private async Task<bool> TaskHasTimeOut(Task task, int millisecondTimeOut = 200)
        {
            var ts = TimeSpan.FromMilliseconds(millisecondTimeOut);
            var timeoutTask = Task.Delay(ts);

            var taskComplete = await Task.WhenAny(task, timeoutTask);
            return taskComplete == timeoutTask;
        }
    }
}
