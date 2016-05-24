using System;
using NSubstitute;
using RateLimiter;
using Xunit;

namespace RateLimiterTest
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
