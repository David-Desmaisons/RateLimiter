using NSubstitute;
using RateLimiter;
using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace RateLimiterTest
{
    public class RateLimiterTest
    {
        private readonly RateLimiter.TimeLimiter _TimeConstraint;
        private readonly IAwaitableConstraint _IAwaitableConstraint;
        private readonly Func<Task> _FuncTask;
        private readonly Func<Task<int>> _FuncTaskInt;

        public RateLimiterTest()
        {
            _FuncTask = Substitute.For<Func<Task>>();
            _FuncTaskInt = Substitute.For<Func<Task<int>>>();
            _IAwaitableConstraint = Substitute.For<IAwaitableConstraint>();
            _TimeConstraint = new RateLimiter.TimeLimiter(_IAwaitableConstraint);
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
            _FuncTask.When(ft => ft.Invoke()).Do(_ => { throw new Exception(); });

            Func<Task> act = async () => await _TimeConstraint.Perform(_FuncTask);
            act.ShouldThrow<Exception>();
        }

        [Fact]
        public async Task Perform_CallExcecuteInCaseOfException()
        {
            _FuncTask.When(ft => ft.Invoke()).Do(_ => { throw new Exception(); });
            try
            {
                await _TimeConstraint.Perform(_FuncTask);
            }
            catch
            {
            }

            CheckSequence();
        }

        private void CheckSequence()
        {
            Received.InOrder(() => {
                _IAwaitableConstraint.WaitForReadiness();
                _FuncTask.Invoke();
                _IAwaitableConstraint.Execute();
            });
        }

        [Fact]
        public async Task PerformGeneric_CallFuncAndIAwaitableConstraintMethods()
        {
            await _TimeConstraint.Perform(_FuncTaskInt);

            CheckGenericSequence();
        }

        [Fact]
        public void PerformGeneric_InCaseOfException_rethrowException()
        {
            _FuncTaskInt.When(ft => ft.Invoke()).Do(_ => { throw new Exception(); });

            Func<Task> act = async () => await _TimeConstraint.Perform(_FuncTaskInt);
            act.ShouldThrow<Exception>();
        }

        [Fact]
        public async Task PerformGeneric_CallExcecuteInCaseOfException()
        {
            _FuncTaskInt.When(ft => ft.Invoke()).Do(_ => { throw new Exception(); });
            try
            {
                await _TimeConstraint.Perform(_FuncTaskInt);
            }
            catch
            {
            }

            CheckGenericSequence();
        }

        private void CheckGenericSequence()
        {
            Received.InOrder(() => {
                _IAwaitableConstraint.WaitForReadiness();
                _FuncTaskInt.Invoke();
                _IAwaitableConstraint.Execute();
            });
        }
    }
}
