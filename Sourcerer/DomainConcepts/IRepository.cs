using System;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.DomainConcepts.Queries;

namespace Sourcerer.DomainConcepts
{
    public interface IRepository<T> where T : IAggregateRoot
    {
        T GetById(Guid id);
        T TryGetById(Guid id);
        void Add(T item);
        void Remove(T item);

        T[] Query(IQuery<T> query);
        TProjection Query<TProjection>(IQuery<T, TProjection> query);
    }
}