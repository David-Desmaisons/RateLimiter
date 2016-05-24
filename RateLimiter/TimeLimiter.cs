using System;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter
{
    public class TimeLimiter : IRateLimiter
    {
        private readonly IAwaitableConstraint _AwaitableConstraint;

        internal TimeLimiter(IAwaitableConstraint awaitableConstraint)
        {
            _AwaitableConstraint = awaitableConstraint;
        }

        public Task Perform(Func<Task> perform) 
        {
            return Perform(perform, CancellationToken.None);
        }

        public Task<T> Perform<T>(Func<Task<T>> perform) 
        {
            return Perform(perform, CancellationToken.None);
        }

        public async Task Perform(Func<Task> perform, CancellationToken cancellationToken) 
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (await _AwaitableConstraint.WaitForReadiness(cancellationToken)) 
            {
                await perform();
            }
        }

        public async Task<T> Perform<T>(Func<Task<T>> perform, CancellationToken cancellationToken) 
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (await _AwaitableConstraint.WaitForReadiness(cancellationToken)) 
            {
                return await perform();
            }
        }

        public static TimeLimiter GetFromMaxCountByInterval(int maxCount, TimeSpan timeSpan)
        {
            return new TimeLimiter(new CountByIntervalAwaitableConstraint(maxCount, timeSpan));
        }

        public static TimeLimiter Compose(params IAwaitableConstraint[] constraints)
        {
            IAwaitableConstraint current = null;
            foreach (var constraint in constraints)
            {
                current = (current == null) ? constraint : current.Compose(constraint);
            }
            return new TimeLimiter(current);
        }
    }
}
