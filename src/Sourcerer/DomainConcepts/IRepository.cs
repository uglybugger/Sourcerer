using System;
using System.Linq;
using Sourcerer.DomainConcepts.Entities;

namespace Sourcerer.DomainConcepts
{
    public interface IRepository<T> where T : IAggregateRoot
    {
        T GetById(Guid id);
        void Add(T item);
        void Remove(T item);
        T[] Query(Func<IQueryable<T>, T[]> query);
    }
}