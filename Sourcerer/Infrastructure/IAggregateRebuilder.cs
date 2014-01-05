using System;
using Sourcerer.DomainConcepts.Entities;

namespace Sourcerer.Infrastructure
{
    public interface IAggregateRebuilder
    {
        T Rebuild<T>(Guid id) where T : IAggregateRoot;
    }
}