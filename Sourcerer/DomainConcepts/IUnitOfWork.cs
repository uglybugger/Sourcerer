using System;
using Sourcerer.DomainConcepts.Entities;

namespace Sourcerer.DomainConcepts
{
    public interface IUnitOfWork : IDisposable
    {
        T TryGetEnlistedAggregateRoot<T>(Guid id) where T : IAggregateRoot;
        void EnlistInTransaction(IAggregateRoot item);
        void Commit();
    }
}