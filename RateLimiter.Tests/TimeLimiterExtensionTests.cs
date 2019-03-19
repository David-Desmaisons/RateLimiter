using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace RateLimiter.Tests
{
    public class TimeLimiterExtensionTests
    {
        private readonly TimeLimiter _TimeConstraint;
        private readonly IAwaitableConstraint _IAwaitableConstraint;
        private readonly IDisposable _Disposable;

        public TimeLimiterExtensionTests()
        {
            _IAwaitableConstraint = Substitute.For<IAwaitableConstraint>();
            _Disposable = Substitute.For<IDisposable>();
            _IAwaitableConstraint.WaitForReadiness(Arg.Any<CancellationToken>()).Returns(Task.FromResult(_Disposable));
            _TimeConstraint = new TimeLimiter(_IAwaitableConstraint);
        }

        [Fact]
        public async Task GetAwaiter()
        {
            await _TimeConstraint;

            CheckAwaitSequence();
        }

        [Fact]
        public async Task GetAwaiter_Call_WaitForReadiness()
        {
            await _TimeConstraint;

            await _IAwaitableConstraint.Received(1).WaitForReadiness(CancellationToken.None);
        }

        private void CheckAwaitSequence()
        {
            Received.InOrder(() =>
            {
                _IAwaitableConstraint.WaitForReadiness(CancellationToken.None);
                _Disposable.Dispose();
            });
        }
    }
}
