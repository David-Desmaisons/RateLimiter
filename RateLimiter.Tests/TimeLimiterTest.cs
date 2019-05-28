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
        public async Task Enqueue_CallFuncAndIAwaitableConstraintMethods()
        {
            await _TimeConstraint.Enqueue(_FuncTask);

            CheckSequence();
        }

        [Fact]
        public void Enqueue_InCaseOfException_rethrowException()
        {
            _FuncTask.When(ft => ft.Invoke()).Do(_ => throw new Exception());

            Func<Task> act = async () => await _TimeConstraint.Enqueue(_FuncTask);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public async Task Enqueue_CallExcecuteInCaseOfException()
        {
            _FuncTask.When(ft => ft.Invoke()).Do(_ => throw new Exception());
            try
            {
                await _TimeConstraint.Enqueue(_FuncTask);
            }
            catch
            {
            }

            CheckSequence();
        }

        [Fact]
        public void Enqueue_WhenCancelled_ThrowException()
        {
            Func<Task> act = async () => await _TimeConstraint.Enqueue(_FuncTask, new CancellationToken(true));
            act.Should().Throw<TaskCanceledException>();
        }

        [Fact]
        public async Task Enqueue_WhenCancelled_DoNotCallFunction()
        {
            try
            {
                await _TimeConstraint.Enqueue(_FuncTask, new CancellationToken(true));
            }
            catch
            {
            }
            await _FuncTask.DidNotReceive().Invoke();
        }

        [Fact]
        public void Enqueue_WhenAwaitableConstraintIsCancelled_ThrowException()
        {
            SetUpAwaitableConstraintIsCancelled();
            Func<Task> act = async () => await _TimeConstraint.Enqueue(_FuncTask, new CancellationToken(true));
            act.Should().Throw<TaskCanceledException>();
        }

        [Fact]
        public async Task Enqueue_WhenAwaitableConstraintIsCancelled_DoNotCallFunction()
        {
            SetUpAwaitableConstraintIsCancelled();
            try
            {
                await _TimeConstraint.Enqueue(_FuncTask, new CancellationToken(true));
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
        public async Task EnqueueGeneric_CallFuncAndIAwaitableConstraintMethods()
        {
            await _TimeConstraint.Enqueue(_FuncTaskInt);

            CheckGenericSequence();
        }

        [Fact]
        public async Task EnqueueGeneric_Returns_Func_result()
        {
            _FuncTaskInt().Returns(Task.FromResult(555));
            var res = await _TimeConstraint.Enqueue(_FuncTaskInt);
            res.Should().Be(555);
        }

        [Fact]
        public void EnqueueGeneric_InCaseOfException_rethrowException()
        {
            _FuncTaskInt.When(ft => ft.Invoke()).Do(_ => throw new Exception());

            Func<Task> act = async () => await _TimeConstraint.Enqueue(_FuncTaskInt);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public async Task EnqueueGeneric_CallExcecuteInCaseOfException()
        {
            _FuncTaskInt.When(ft => ft.Invoke()).Do(_ => throw new Exception());
            try
            {
                await _TimeConstraint.Enqueue(_FuncTaskInt);
            }
            catch
            {
            }

            CheckGenericSequence();
        }

        [Fact]
        public void EnqueueGeneric_WhenCancelled_ThrowException()
        {
            Func<Task> act = async () => await _TimeConstraint.Enqueue(_FuncTaskInt, new CancellationToken(true));
            act.Should().Throw<TaskCanceledException>();
        }

        [Fact]
        public async Task EnqueueGeneric_WhenCancelled_DoNotCallFunction()
        {
            try
            {
                await _TimeConstraint.Enqueue(_FuncTaskInt, new CancellationToken(true));
            }
            catch
            {
            }
            await _FuncTaskInt.DidNotReceive().Invoke();
        }

        [Fact]
        public async Task EnqueueGeneric_WhenAwaitableConstraintIsCancelled_DoNotCallFunction()
        {
            SetUpAwaitableConstraintIsCancelled();
            try
            {
                await _TimeConstraint.Enqueue(_FuncTaskInt, new CancellationToken(true));
            }
            catch
            {
            }
            await _FuncTaskInt.DidNotReceive().Invoke();
        }

        [Fact]
        public void EnqueueGeneric_WhenAwaitableConstraintIsCancelled_ThrowException()
        {
            SetUpAwaitableConstraintIsCancelled();
            Func<Task> act = async () => await _TimeConstraint.Enqueue(_FuncTaskInt, new CancellationToken(true));
            act.Should().Throw<TaskCanceledException>();
        }

        [Fact]
        public async Task EnqueueGeneric_WithoutTask_CallFuncAndIAwaitableConstraintMethods()
        {
            await _TimeConstraint.Enqueue(_FuncInt);

            CheckGenericSequence_WithoutTask();
        }

        [Fact]
        public async Task EnqueueGeneric_WithoutTask_Returns_Func_result()
        {
            _FuncInt().Returns(8889);
            var res = await _TimeConstraint.Enqueue(_FuncInt);
            res.Should().Be(8889);
        }

        [Fact]
        public void EnqueueGeneric_WithoutTask_InCaseOfException_rethrowException()
        {
            _FuncInt.When(ft => ft.Invoke()).Do(_ => throw new Exception());

            Func<Task> act = async () => await _TimeConstraint.Enqueue(_FuncInt);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public async Task EnqueueGeneric_WithoutTask_CallExcecuteInCaseOfException()
        {
            _FuncInt.When(ft => ft.Invoke()).Do(_ => throw new Exception());
            try
            {
                await _TimeConstraint.Enqueue(_FuncInt);
            }
            catch
            {
            }

            CheckGenericSequence_WithoutTask();
        }

        [Fact]
        public void EnqueueGeneric_WithoutTask_WhenCancelled_ThrowException()
        {
            Func<Task> act = async () => await _TimeConstraint.Enqueue(_FuncInt, new CancellationToken(true));
            act.Should().Throw<TaskCanceledException>();
        }

        [Fact]
        public async void EnqueueGeneric_WithoutTask_WhenCancelled_DoNotCallFunction()
        {
            try
            {
                await _TimeConstraint.Enqueue(_FuncInt, new CancellationToken(true));
            }
            catch
            {
            }
            _FuncInt.DidNotReceive().Invoke();
        }

        [Fact]
        public async Task EnqueueGeneric_WithoutTask_WhenAwaitableConstraintIsCancelled_DoNotCallFunction()
        {
            SetUpAwaitableConstraintIsCancelled();
            try
            {
                await _TimeConstraint.Enqueue(_FuncInt, new CancellationToken(true));
            }
            catch
            {
            }
            _FuncInt.DidNotReceive().Invoke();
        }

        [Fact]
        public void EnqueueGeneric_WithoutTask_WhenAwaitableConstraintIsCancelled_ThrowException()
        {
            SetUpAwaitableConstraintIsCancelled();
            Func<Task> act = async () => await _TimeConstraint.Enqueue(_FuncInt, new CancellationToken(true));
            act.Should().Throw<TaskCanceledException>();
        }

        [Fact]
        public async Task Compose_composes_the_contraints()
        {
            var constraints = Enumerable.Range(0, 3).Select(_ => GetSubstituteAwaitableConstraint()).ToArray();
            var composed = TimeLimiter.Compose(constraints);

            await composed.Enqueue(() => { });

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
