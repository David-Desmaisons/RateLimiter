using NSubstitute;
using RateLimiter;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace RateLimiterTest
{
    public class ComposedAwaitableConstraintTest
    {
        private readonly IAwaitableConstraint _IAwaitableConstraint1;
        private readonly IAwaitableConstraint _IAwaitableConstraint2;
        private readonly IDisposable _Diposable1;
        private readonly IDisposable _Diposable2;
        private readonly ComposedAwaitableConstraint _Composed;

        public ComposedAwaitableConstraintTest()
        {
            _IAwaitableConstraint1 = Substitute.For<IAwaitableConstraint>();
            _IAwaitableConstraint2 = Substitute.For<IAwaitableConstraint>();
            _Diposable1 = Substitute.For<IDisposable>();
            _Diposable2 = Substitute.For<IDisposable>();
            _IAwaitableConstraint1.WaitForReadiness(Arg.Any<CancellationToken>()).Returns(Task.FromResult(_Diposable1));
            _IAwaitableConstraint2.WaitForReadiness(Arg.Any<CancellationToken>()).Returns(Task.FromResult(_Diposable2));
            _Composed = new ComposedAwaitableConstraint(_IAwaitableConstraint1, _IAwaitableConstraint2);
        }

        [Fact]
        public async Task WaitForReadiness_Call_ComposingElementsWaitForReadiness()
        {
            await _Composed.WaitForReadiness(CancellationToken.None);

            await _IAwaitableConstraint1.Received(1).WaitForReadiness(CancellationToken.None);
            await _IAwaitableConstraint2.Received(1).WaitForReadiness(CancellationToken.None);
        }

        [Fact]
        public async Task WaitForReadiness_Block()
        {
            await _Composed.WaitForReadiness(CancellationToken.None);
            var timedOut = await WaitForReadinessHasTimeOut();
            timedOut.Should().BeTrue();
        }

        [Fact]
        public async Task WaitForReadiness_BlockUntillExecuteIsCalled()
        {
            using (await _Composed.WaitForReadiness(CancellationToken.None)) {              
            }  
            var timedOut = await WaitForReadinessHasTimeOut();
            timedOut.Should().BeFalse();
        }

        private async Task<bool> WaitForReadinessHasTimeOut()
        {
            var task = _Composed.WaitForReadiness(CancellationToken.None);
            var timeoutTask = Task.Delay(TimeSpan.FromMilliseconds(200));

            var taskcomplete = await Task.WhenAny(task, timeoutTask);
            return taskcomplete == timeoutTask;
        }

        [Fact]
        public async Task Execute_Call_ComposingElementsExecute()
        {
            var disp = await _Composed.WaitForReadiness(CancellationToken.None);
            disp.Dispose();

            _Diposable1.Received(1).Dispose();
            _Diposable2.Received(1).Dispose();
        }
    }
}
