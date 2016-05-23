using System;
using System.Threading.Tasks;

namespace RateLimiter
{
    public interface IRateLimiter
    {
        Task Perform(Func<Task> perform);

        Task<T> Perform<T>(Func<Task<T>> perform);
    }
}
