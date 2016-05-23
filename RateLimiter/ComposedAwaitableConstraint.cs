using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter
{
    public class ComposedAwaitableConstraint : IAwaitableConstraint
    {
        private IAwaitableConstraint _AwaitableConstraint1;
        private IAwaitableConstraint _AwaitableConstraint2;
        private readonly SemaphoreSlim _Semafore = new SemaphoreSlim(1, 1);

        internal ComposedAwaitableConstraint(IAwaitableConstraint awaitableConstraint1, IAwaitableConstraint awaitableConstraint2)
        {
            _AwaitableConstraint1 = awaitableConstraint1;
            _AwaitableConstraint2 = awaitableConstraint2;
        }

        public async Task WaitForReadiness()
        {
            await _Semafore.WaitAsync();
            await Task.WhenAll(_AwaitableConstraint1.WaitForReadiness(), _AwaitableConstraint2.WaitForReadiness());
        }

        public void Execute()
        {
            _AwaitableConstraint1.Execute();
            _AwaitableConstraint2.Execute();
            _Semafore.Release();
        }
    }
}
