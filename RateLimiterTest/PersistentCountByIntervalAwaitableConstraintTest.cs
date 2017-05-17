using RateLimiter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using FluentAssertions;
using System.Threading.Tasks;

namespace RateLimiterTest
{
    public class PersistentCountByIntervalAwaitableConstraintTest
    {
        [Fact]
        public async Task WaitForReadiness_WithoutInitialState()
        {
            var log = new List<DateTime>();

            var constraint = new PersistentCountByIntervalAwaitableConstraint(1, TimeSpan.FromMilliseconds(200),
                timeStamp => log.Add(timeStamp), null);

            for (int i = 0; i < 5; i++)
            {
                (await constraint.WaitForReadiness(CancellationToken.None)).Dispose();
            }

            log.Should().HaveCount(5);
        }

        [Fact]
        public async Task WaitForReadiness_WithInitialState()
        {
            var firstTimeStamp = new DateTime(2000, 1, 1);
            var secondTimeStamp = new DateTime(2001, 1, 1);
            var log = new List<DateTime> {firstTimeStamp, secondTimeStamp};

            var constraint = new PersistentCountByIntervalAwaitableConstraint(7, TimeSpan.FromSeconds(1),
                timeStamp => log.Add(timeStamp), log);

            for (int i = 0; i < 5; i++)
            {
                (await constraint.WaitForReadiness(CancellationToken.None)).Dispose();
            }

            log.Should().HaveCount(7);
            log[0].Should().Be(firstTimeStamp);
            log[1].Should().Be(secondTimeStamp);
        }
    }
}
