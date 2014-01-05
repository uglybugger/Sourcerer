using System;
using System.Collections.Generic;
using System.Linq;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.DomainConcepts.Queries
{
    public interface IQueryableSnapshot
    {
        T GetById<T>(Guid id) where T : IAggregateRoot;
        T TryGetById<T>(Guid id) where T : IAggregateRoot;
        IQueryable<T> Items<T>() where T : IAggregateRoot;
        void NotifyFactsWereCommitted(IEnumerable<IFact> facts);    //FIXME icky :(
    }
}