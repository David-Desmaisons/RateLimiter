using System;
using System.Collections.Generic;
using System.Text;

namespace RateLimiter
{
    /// <summary>
    /// Provides extension to interface <see cref="TimeLimiter"/>
    /// </summary>
    public static class TimeLimiterExtension
    {
        /// <summary>
        /// Gets an awaiter used to await this <see cref="TimeLimiter"/>.
        /// </summary>
        /// <returns>An awaiter instance.</returns>
        /// <remarks>This method is intended for compiler use rather than for use in application code.</remarks>
        public static TimeLimiterAwaiter GetAwaiter(this TimeLimiter timeLimiter)
        {
            return new TimeLimiterAwaiter(timeLimiter);
        }
    }
}
