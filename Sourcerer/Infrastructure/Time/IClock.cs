using System;

namespace Sourcerer.Infrastructure.Time
{
    public interface IClock
    {
        DateTimeOffset UtcNow { get; }
    }
}