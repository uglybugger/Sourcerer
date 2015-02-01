using System;
using Sourcerer.DomainConcepts.Entities;

namespace Sourcerer.Infrastructure
{
    public interface IAggregateRebuilder
    {
        T Rebuild<T>(Guid id) where T : class, IAggregateRoot;
        T[] RebuildAll<T>() where T : class, IAggregateRoot;
    }
}