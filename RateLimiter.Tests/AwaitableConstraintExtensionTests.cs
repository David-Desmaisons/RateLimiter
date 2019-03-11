using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace RateLimiter.Tests
{
    public class AwaitableConstraintExtensionTests
    {
        private readonly IAwaitableConstraint _AwaitableConstraint1;
        private readonly IAwaitableConstraint _AwaitableConstraint2;
        private readonly IDisposable _Disposable1;
        private readonly IDisposable _Disposable2;
        private readonly IAwaitableConstraint _Composed;

        public AwaitableConstraintExtensionTests()
        {
            _AwaitableConstraint1 = Substitute.For<IAwaitableConstraint>();
            _AwaitableConstraint2 = Substitute.For<IAwaitableConstraint>();
            _Disposable1 = Substitute.For<IDisposable>();
            _Disposable2 = Substitute.For<IDisposable>();
            _AwaitableConstraint1.WaitForReadiness(Arg.Any<CancellationToken>()).Returns(Task.FromResult(_Disposable1));
            _AwaitableConstraint2.WaitForReadiness(Arg.Any<CancellationToken>()).Returns(Task.FromResult(_Disposable2));
            _Composed = _AwaitableConstraint1.Compose(_AwaitableConstraint2);
        }


        [Fact]
        public void Compose_creates_an_instance_of_ComposedAwaitableConstraint()
        {
            _Composed.Should().BeOfType<ComposedAwaitableConstraint>();
        }

        [Fact]
        public void Compose_returns_self_if_constraint_is_the_same()
        {
            var composed = _AwaitableConstraint1.Compose(_AwaitableConstraint1);
            composed.Should().Be(_AwaitableConstraint1);
        }

        [Fact]
        public async Task Compose_Use_both_AwaitableConstraints()
        {
            await _Composed.WaitForReadiness(CancellationToken.None);

            await _AwaitableConstraint1.Received(1).WaitForReadiness(CancellationToken.None);
            await _AwaitableConstraint2.Received(1).WaitForReadiness(CancellationToken.None);
        }
    }
}
