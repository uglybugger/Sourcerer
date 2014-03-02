using System;

namespace Sourcerer.Infrastructure
{
    public interface ITypesProvider
    {
        Type[] AggregateTypes { get; }
        Type[] FactTypes { get; }
    }
}