using System;
using System.Collections.Generic;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.Infrastructure
{
    public interface IFactStore
    {
        void AppendAtomically(IFact[] facts);

        IEnumerable<FactAbout<T>> GetStream<T>(Guid id) where T : IAggregateRoot;
        IEnumerable<Guid> GetAllStreamIds<T>() where T : IAggregateRoot;
    }
}