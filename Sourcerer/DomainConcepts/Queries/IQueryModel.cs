using System;
using System.Collections.Generic;
using System.Linq;
using Sourcerer.DomainConcepts.Entities;

namespace Sourcerer.DomainConcepts.Queries
{
    public interface IQueryModel<T> where T : class, IAggregateRoot
    {
        T GetById(Guid id);

        IQueryable<T> Items { get; }

        void UpdateAtomically(IEnumerable<T> newItems, IEnumerable<T> modifiedItems, IEnumerable<T> removedItems);
    }
}