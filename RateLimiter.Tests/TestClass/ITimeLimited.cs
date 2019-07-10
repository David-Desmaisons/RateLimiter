using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter.Tests.TestClass
{
    public interface ITimeLimited
    {
        Task<int> GetValue();

        Task<int> GetValue(CancellationToken token);
    }
}
