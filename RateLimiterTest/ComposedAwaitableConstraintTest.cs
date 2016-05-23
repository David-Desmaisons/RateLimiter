using NSubstitute;
using RateLimiter;
using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace RateLimiterTest
{
    public class ComposedAwaitableConstraintTest
    {
        private readonly IAwaitableConstraint _IAwaitableConstraint1;
        private readonly IAwaitableConstraint _IAwaitableConstraint2;
        private readonly ComposedAwaitableConstraint _Composed;

        public ComposedAwaitableConstraintTest()
        {
            _IAwaitableConstraint1 = Substitute.For<IAwaitableConstraint>();
            _IAwaitableConstraint2 = Substitute.For<IAwaitableConstraint>();
            _Composed = new ComposedAwaitableConstraint(_IAwaitableConstraint1, _IAwaitableConstraint2);
        }

        [Fact]
        public async Task WaitForReadiness_Call_ComposingElementsWaitForReadiness()
        {
            await _Composed.WaitForReadiness();

            await _IAwaitableConstraint1.Received(1).WaitForReadiness();
            await _IAwaitableConstraint2.Received(1).WaitForReadiness();
        }

        [Fact]
        public async Task WaitForReadiness_Block()
        {
            await _Composed.WaitForReadiness();
            var timedOut = await WaitForReadinessHasTimeOut();
            timedOut.Should().BeTrue();
        }

        [Fact]
        public async Task WaitForReadiness_BlockUntillExecuteIsCalled()
        {
            await _Composed.WaitForReadiness();
            _Composed.Execute();     
            var timedOut = await WaitForReadinessHasTimeOut();
            timedOut.Should().BeFalse();
        }

        private async Task<bool> WaitForReadinessHasTimeOut()
        {
            var task = _Composed.WaitForReadiness();
            var timeoutTask = Task.Delay(TimeSpan.FromMilliseconds(200));

            var taskcomplete = await Task.WhenAny(task, timeoutTask);
            return taskcomplete == timeoutTask;
        }

        [Fact]
        public async Task Execute_Call_ComposingElementsExecute()
        {
            await _Composed.WaitForReadiness();
            _IAwaitableConstraint1.ClearReceivedCalls();
            _IAwaitableConstraint2.ClearReceivedCalls();

            _Composed.Execute();

            _IAwaitableConstraint1.Received(1).Execute();
            _IAwaitableConstraint2.Received(1).Execute();
        }
    }
}
