using System.Threading.Tasks;

namespace RateLimiter
{
    public interface IAwaitableConstraint
    {
        Task WaitForReadiness();

        void Execute();
    }
}
