using System;

namespace RateLimiter 
{
    public class DisposeAction : IDisposable
    {
        private Action _Act;

        public DisposeAction(Action act) 
        {
            _Act = act;
        }

        public void Dispose() 
        {
            if (_Act != null)
                _Act();

            _Act = null;
        }
    }
}
