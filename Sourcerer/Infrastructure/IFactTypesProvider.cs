using System;

namespace Sourcerer.Infrastructure
{
    public interface IFactTypesProvider
    {
        Type[] FactTypes { get; }
    }
}