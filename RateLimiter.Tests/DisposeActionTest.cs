using System;
using NSubstitute;
using Xunit;

namespace RateLimiter.Tests
{
    public class DisposeActionTest
    {
        private readonly DisposeAction _DisposeAction;
        private readonly Action _Action;

        public DisposeActionTest()
        {
            _Action = Substitute.For<Action>();
            _DisposeAction = new DisposeAction(_Action);
        }

        [Fact]
        public void Dispose_CallAction() 
        {
            _DisposeAction.Dispose();

            _Action.Received(1).Invoke();
        }

        [Fact]
        public void Dispose_CallActionOnlyOnce() 
        {
            _DisposeAction.Dispose();
            _DisposeAction.Dispose();

            _Action.Received(1).Invoke();
        }
    }
}
