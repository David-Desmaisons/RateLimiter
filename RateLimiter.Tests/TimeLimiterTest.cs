using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace RateLimiter.Tests
{
    public class TimeLimiterTest
    {
        private readonly TimeLimiter _TimeConstraint;
        private readonly IAwaitableConstraint _IAwaitableConstraint;
        private readonly Func<Task> _FuncTask;
        private readonly Func<Task<int>> _FuncTaskInt;
        private readonly Func<int> _FuncInt;
        private readonly IDisposable _Diposable;

        public TimeLimiterTest()
        {
            _FuncTask = Substitute.For<Func<Task>>();
            _FuncInt = Substitute.For<Func<int>>();
            _FuncTaskInt = Substitute.For<Func<Task<int>>>();
            _IAwaitableConstraint = Substitute.For<IAwaitableConstraint>();
            _Diposable = Substitute.For<IDisposable>();
            _IAwaitableConstraint.WaitForReadiness(Arg.Any<CancellationToken>()).Returns(Task.FromResult(_Diposable));
            _TimeConstraint = new TimeLimiter(_IAwaitableConstraint);
        }

        [Fact]
        public async Task Perform_CallFuncAndIAwaitableConstraintMethods()
        {
            await _TimeConstraint.Perform(_FuncTask);

            CheckSequence();
        }

        [Fact]
        public void Perform_InCaseOfException_rethrowException()
        {
            _FuncTask.When(ft => ft.Invoke()).Do(_ => throw new Exception());

            Func<Task> act = async () => await _TimeConstraint.Perform(_FuncTask);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public async Task Perform_CallExcecuteInCaseOfException()
        {
            _FuncTask.When(ft => ft.Invoke()).Do(_ => throw new Exception());
            try
            {
                await _TimeConstraint.Perform(_FuncTask);
            }
            catch
            {
            }

            CheckSequence();
        }

        [Fact]
        public void Perform_WhenCancelled_ThrowException()
        {
            Func<Task> act = async () => await _TimeConstraint.Perform(_FuncTask, new CancellationToken(true));
            act.Should().Throw<TaskCanceledException>();
        }

        [Fact]
        public async Task Perform_WhenCancelled_DoNotCallFunction()
        {
            try
            {
                await _TimeConstraint.Perform(_FuncTask, new CancellationToken(true));
            }
            catch
            {
            }
            await _FuncTask.DidNotReceive().Invoke();
        }

        [Fact]
        public void Perform_WhenAwaitableConstraintIsCancelled_ThrowException()
        {
            SetUpAwaitableConstraintIsCancelled();
            Func<Task> act = async () => await _TimeConstraint.Perform(_FuncTask, new CancellationToken(true));
            act.Should().Throw<TaskCanceledException>();
        }

        [Fact]
        public async Task Perform_WhenAwaitableConstraintIsCancelled_DoNotCallFunction()
        {
            SetUpAwaitableConstraintIsCancelled();
            try
            {
                await _TimeConstraint.Perform(_FuncTask, new CancellationToken(true));
            }
            catch
            {
            }
            await _FuncTask.DidNotReceive().Invoke();
        }

        private void CheckSequence()
        {
            Received.InOrder(() => {
                _IAwaitableConstraint.WaitForReadiness(CancellationToken.None);
                _FuncTask.Invoke();
                _Diposable.Dispose();
            });
        }

        [Fact]
        public async Task PerformGeneric_CallFuncAndIAwaitableConstraintMethods()
        {
            await _TimeConstraint.Perform(_FuncTaskInt);

            CheckGenericSequence();
        }

        [Fact]
        public async Task PerformGeneric_Returns_Func_result()
        {
            _FuncTaskInt().Returns(Task.FromResult(555));
            var res = await _TimeConstraint.Perform(_FuncTaskInt);
            res.Should().Be(555);
        }

        [Fact]
        public void PerformGeneric_InCaseOfException_rethrowException()
        {
            _FuncTaskInt.When(ft => ft.Invoke()).Do(_ => throw new Exception());

            Func<Task> act = async () => await _TimeConstraint.Perform(_FuncTaskInt);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public async Task PerformGeneric_CallExcecuteInCaseOfException()
        {
            _FuncTaskInt.When(ft => ft.Invoke()).Do(_ => throw new Exception());
            try
            {
                await _TimeConstraint.Perform(_FuncTaskInt);
            }
            catch
            {
            }

            CheckGenericSequence();
        }

        [Fact]
        public void PerformGeneric_WhenCancelled_ThrowException()
        {
            Func<Task> act = async () => await _TimeConstraint.Perform(_FuncTaskInt, new CancellationToken(true));
            act.Should().Throw<TaskCanceledException>();
        }

        [Fact]
        public async Task PerformGeneric_WhenCancelled_DoNotCallFunction()
        {
            try
            {
                await _TimeConstraint.Perform(_FuncTaskInt, new CancellationToken(true));
            }
            catch
            {
            }
            await _FuncTaskInt.DidNotReceive().Invoke();
        }

        [Fact]
        public async Task PerformGeneric_WhenAwaitableConstraintIsCancelled_DoNotCallFunction()
        {
            SetUpAwaitableConstraintIsCancelled();
            try
            {
                await _TimeConstraint.Perform(_FuncTaskInt, new CancellationToken(true));
            }
            catch
            {
            }
            await _FuncTaskInt.DidNotReceive().Invoke();
        }

        [Fact]
        public void PerformGeneric_WhenAwaitableConstraintIsCancelled_ThrowException()
        {
            SetUpAwaitableConstraintIsCancelled();
            Func<Task> act = async () => await _TimeConstraint.Perform(_FuncTaskInt, new CancellationToken(true));
            act.Should().Throw<TaskCanceledException>();
        }

        [Fact]
        public async Task PerformGeneric_WithoutTask_CallFuncAndIAwaitableConstraintMethods()
        {
            await _TimeConstraint.Perform(_FuncInt);

            CheckGenericSequence_WithoutTask();
        }

        [Fact]
        public async Task PerformGeneric_WithoutTask_Returns_Func_result()
        {
            _FuncInt().Returns(8889);
            var res = await _TimeConstraint.Perform(_FuncInt);
            res.Should().Be(8889);
        }

        [Fact]
        public void PerformGeneric_WithoutTask_InCaseOfException_rethrowException()
        {
            _FuncInt.When(ft => ft.Invoke()).Do(_ => throw new Exception());

            Func<Task> act = async () => await _TimeConstraint.Perform(_FuncInt);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public async Task PerformGeneric_WithoutTask_CallExcecuteInCaseOfException()
        {
            _FuncInt.When(ft => ft.Invoke()).Do(_ => throw new Exception());
            try
            {
                await _TimeConstraint.Perform(_FuncInt);
            }
            catch
            {
            }

            CheckGenericSequence_WithoutTask();
        }

        [Fact]
        public void PerformGeneric_WithoutTask_WhenCancelled_ThrowException()
        {
            Func<Task> act = async () => await _TimeConstraint.Perform(_FuncInt, new CancellationToken(true));
            act.Should().Throw<TaskCanceledException>();
        }

        [Fact]
        public async void PerformGeneric_WithoutTask_WhenCancelled_DoNotCallFunction()
        {
            try
            {
                await _TimeConstraint.Perform(_FuncInt, new CancellationToken(true));
            }
            catch
            {
            }
            _FuncInt.DidNotReceive().Invoke();
        }

        [Fact]
        public async Task PerformGeneric_WithoutTask_WhenAwaitableConstraintIsCancelled_DoNotCallFunction()
        {
            SetUpAwaitableConstraintIsCancelled();
            try
            {
                await _TimeConstraint.Perform(_FuncInt, new CancellationToken(true));
            }
            catch
            {
            }
            _FuncInt.DidNotReceive().Invoke();
        }

        [Fact]
        public void PerformGeneric_WithoutTask_WhenAwaitableConstraintIsCancelled_ThrowException()
        {
            SetUpAwaitableConstraintIsCancelled();
            Func<Task> act = async () => await _TimeConstraint.Perform(_FuncInt, new CancellationToken(true));
            act.Should().Throw<TaskCanceledException>();
        }

        [Fact]
        public async Task Compose_composes_the_contraints()
        {
            var constraints = Enumerable.Range(0, 3).Select(_ => GetSubstituteAwaitableConstraint()).ToArray();
            var composed = TimeLimiter.Compose(constraints);

            await composed.Perform(() => { });

            await Task.WhenAll(
                constraints.Select(c => c.Received().WaitForReadiness(Arg.Any<CancellationToken>())).ToArray()
            );
        }

        private static IAwaitableConstraint GetSubstituteAwaitableConstraint()
        {
            var constraint = Substitute.For<IAwaitableConstraint>();
            var disposable = Substitute.For<IDisposable>();
            constraint.WaitForReadiness(Arg.Any<CancellationToken>()).Returns(Task.FromResult(constraint));
            return constraint;
        }

        private void SetUpAwaitableConstraintIsCancelled()
        {
            _IAwaitableConstraint.WaitForReadiness(Arg.Any<CancellationToken>()).Returns(x => throw new TaskCanceledException());
        }

        private void CheckGenericSequence()
        {
            Received.InOrder(() => {
                _IAwaitableConstraint.WaitForReadiness(CancellationToken.None);
                _FuncTaskInt.Invoke();
                _Diposable.Dispose();
            });
        }

        private void CheckGenericSequence_WithoutTask()
        {
            Received.InOrder(() => {
                _IAwaitableConstraint.WaitForReadiness(CancellationToken.None);
                _FuncInt.Invoke();
                _Diposable.Dispose();
            });
        }
    }
}
