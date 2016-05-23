using System;
using System.Threading.Tasks;

namespace RateLimiter
{
    public class TimeLimiter : IRateLimiter {
        private readonly IAwaitableConstraint _AwaitableConstraint;

        internal TimeLimiter(IAwaitableConstraint awaitableConstraint) {
            _AwaitableConstraint = awaitableConstraint;
        }

        public async Task Perform(Func<Task> perform) {
            try {
                await WaitForReadiness();
                await perform();
            }
            finally {
                SignalExecute();
            }
        }

        public async Task<T> Perform<T>(Func<Task<T>> perform) {
            try {
                await WaitForReadiness();
                return await perform();
            }
            finally {
                SignalExecute();
            }
        }

        private async Task WaitForReadiness() {
            await _AwaitableConstraint.WaitForReadiness();
        }

        private void SignalExecute() {
            _AwaitableConstraint.Execute();
        }

        public static TimeLimiter GetFromMaxCountByInterval(int maxCount, TimeSpan timeSpan) 
        {
            return new TimeLimiter(new CountByIntervalAwaitableConstraint(maxCount, timeSpan));
        }
    }
}
