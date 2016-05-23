using RateLimiter;
using System;
using Xunit;
using FluentAssertions;
using System.Threading.Tasks;

namespace RateLimiterTest
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
            TimeSystem.StandardTime = _MockTime;
            _CountByIntervalAwaitableConstraint1 = new CountByIntervalAwaitableConstraint(1, TimeSpan.FromMilliseconds(100));
            _CountByIntervalAwaitableConstraint2 = new CountByIntervalAwaitableConstraint(5, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Constructor_WithNegativeCount_ThrowException()
        {
            Action act = () => new CountByIntervalAwaitableConstraint(-1, TimeSpan.FromSeconds(1));
            act.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void Constructor_WithNegativeTimeSpan_ThrowException()
        {
            Action act = () => new CountByIntervalAwaitableConstraint(10, TimeSpan.FromSeconds(-1));
            act.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public async Task WaitForReadinessIsSynchroneous_FirstTime()
        {
            var task = _CountByIntervalAwaitableConstraint1.WaitForReadiness();
            var timedOut = await TaskHasTimeOut(task, 10);
            timedOut.Should().BeFalse();
        }

        [Fact]
        public async Task WaitForReadinessDoNotCallDelay_FirstTime()
        {
            await _CountByIntervalAwaitableConstraint1.WaitForReadiness();
            _MockTime.GetDelayCount.Should().Be(0);
        }

        [Fact]
        public async Task WaitForReadiness_Block_WhenWaitForReadinessHasBeenCalledButNotExecute()
        {
            await _CountByIntervalAwaitableConstraint1.WaitForReadiness();

            var secondTask = _CountByIntervalAwaitableConstraint1.WaitForReadiness();
            var timedOut = await TaskHasTimeOut(secondTask, 300);
            timedOut.Should().BeTrue();
        }

        [Fact]
        public async Task WaitForReadiness_UnBlock_WhenExcecuteIsCalled()
        {
            await _CountByIntervalAwaitableConstraint1.WaitForReadiness();
            var secondTask = _CountByIntervalAwaitableConstraint1.WaitForReadiness();
            await TaskHasTimeOut(secondTask, 10);
            _CountByIntervalAwaitableConstraint1.Execute();

            var timedOut = await TaskHasTimeOut(secondTask, 300);
            timedOut.Should().BeFalse();
        }

        [Fact]
        public async Task WaitForReadiness_WhenLimitIsReached_CallDelayToRespectTimeConstraint()
        {
            await SetUpSatured();
            await CheckCompletionInTime();
        }

        [Fact]
        public async Task WaitForReadiness_WhenLimitIsReached_CallDelayToRespectTimeConstraintWithMinimalDelay()
        {
            await SetUpSatured();
            _MockTime.AddTime(TimeSpan.FromMilliseconds(50));
            await CheckCompletionInTime();
        }

        [Fact]
        public async Task WaitForReadiness_WhenLimitIsNotReached_DoNotCallDelayToRespectTimeConstraint()
        {
            await SetUpSatured();
            _MockTime.AddTime(TimeSpan.FromMilliseconds(100));
            await _CountByIntervalAwaitableConstraint1.WaitForReadiness();
            _MockTime.GetDelayCount.Should().Be(0);
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(1, false)]
        [InlineData(3, false)]
        [InlineData(2, false)]
        [InlineData(4, false)]
        [InlineData(5, true)]
        public async Task WaitForReadiness_LimitIsBasedOnCount(int count, bool Wait)
        {
            for (int i = 0; i < count; i++)
            {
                await SetUpSatured2();
            }

            await _CountByIntervalAwaitableConstraint2.WaitForReadiness();
            _MockTime.GetDelayCount.Should().Be(Wait ? 1 : 0);
        }

        private async Task SetUpSatured2()
        {
            await _CountByIntervalAwaitableConstraint2.WaitForReadiness();
            _CountByIntervalAwaitableConstraint2.Execute();
        }

        private async Task SetUpSatured()
        {
            await _CountByIntervalAwaitableConstraint1.WaitForReadiness();
            _CountByIntervalAwaitableConstraint1.Execute();
        }

        private async Task CheckCompletionInTime()
        {
            await _CountByIntervalAwaitableConstraint1.WaitForReadiness();
            _MockTime.GetDelayCount.Should().Be(1);
            _MockTime.Now.Should().Be(_Origin.AddMilliseconds(100));
        }

        private async Task<bool> TaskHasTimeOut(Task task, int milliseconTimeOut = 200)
        {
            var ts = TimeSpan.FromMilliseconds(milliseconTimeOut);
            var timeoutTask = Task.Delay(ts);

            var taskcomplete = await Task.WhenAny(task, timeoutTask);
            return taskcomplete == timeoutTask;
        }
    }
}
