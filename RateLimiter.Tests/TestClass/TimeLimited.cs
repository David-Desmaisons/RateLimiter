using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace RateLimiter.Tests.TestClass
{
    public class TimeLimited : ITimeLimited
    {
        private int _Count = 0;
        private readonly ITestOutputHelper _Output;

        public TimeLimited(ITestOutputHelper output)
        {
            _Output = output;
        }

        public async Task<int> GetValue()
        {
            _Output.WriteLine($"{DateTime.Now:MM/dd/yyy HH:mm:ss.fff}");
            _Count++;
            return _Count;
        }

        public async Task<int> GetValue(CancellationToken token) 
        {
            _Output.WriteLine($"{DateTime.Now:MM/dd/yyy HH:mm:ss.fff}");
            _Count++;
            return _Count;
        }
    }
}
