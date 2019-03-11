using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace RateLimiter.Tests
{
    public class ComposedAwaitableConstraintTest
    {
        private readonly IAwaitableConstraint _AwaitableConstraint1;
        private readonly IAwaitableConstraint _AwaitableConstraint2;
        private readonly IDisposable _Disposable1;
        private readonly IDisposable _Disposable2;
        private readonly ComposedAwaitableConstraint _Composed;

        public ComposedAwaitableConstraintTest()
        {
            _AwaitableConstraint1 = Substitute.For<IAwaitableConstraint>();
            _AwaitableConstraint2 = Substitute.For<IAwaitableConstraint>();
            _Disposable1 = Substitute.For<IDisposable>();
            _Disposable2 = Substitute.For<IDisposable>();
            _AwaitableConstraint1.WaitForReadiness(Arg.Any<CancellationToken>()).Returns(Task.FromResult(_Disposable1));
            _AwaitableConstraint2.WaitForReadiness(Arg.Any<CancellationToken>()).Returns(Task.FromResult(_Disposable2));
            _Composed = new ComposedAwaitableConstraint(_AwaitableConstraint1, _AwaitableConstraint2);
        }

        [Fact]
        public async Task WaitForReadiness_Call_ComposingElementsWaitForReadiness()
        {
            await _Composed.WaitForReadiness(CancellationToken.None);

            await _AwaitableConstraint1.Received(1).WaitForReadiness(CancellationToken.None);
            await _AwaitableConstraint2.Received(1).WaitForReadiness(CancellationToken.None);
        }

        [Fact]
        public async Task WaitForReadiness_Block()
        {
            await _Composed.WaitForReadiness(CancellationToken.None);
            var timedOut = await WaitForReadinessHasTimeOut();
            timedOut.Should().BeTrue();
        }

        [Fact]
        public async Task WaitForReadiness_WhenCancelled_DoNotBlock()
        {
            var cancellation = new CancellationToken(true);
            try
            {
                await _Composed.WaitForReadiness(cancellation);
            }
            catch
            {
            }
            var timedOut = await WaitForReadinessHasTimeOut();
            timedOut.Should().BeFalse();
        }

        [Fact]
        public void WaitForReadiness_WhenCancelled_ThrowException()
        {
            var cancellation = new CancellationToken(true);
            Func<Task> act = async () => await _Composed.WaitForReadiness(cancellation);
            act.Should().Throw<TaskCanceledException>();
        }

        [Fact]
        public async Task WaitForReadiness_BlockUntillDisposeIsCalled()
        {
            var disp = await _Composed.WaitForReadiness(CancellationToken.None);
            disp.Dispose();

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

            _Disposable1.Received(1).Dispose();
            _Disposable2.Received(1).Dispose();
        }
    }
}
