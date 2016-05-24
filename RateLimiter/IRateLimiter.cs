using System;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter
{
    public interface IRateLimiter
    {
        Task Perform(Func<Task> perform, CancellationToken cancellationToken);

        Task Perform(Func<Task> perform);

        Task<T> Perform<T>(Func<Task<T>> perform);

        Task<T> Perform<T>(Func<Task<T>> perform, CancellationToken cancellationTokenf);
    }
}
