using System;
using System.Collections.Generic;
using System.Linq;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.DomainConcepts.Facts;
using Sourcerer.DomainConcepts.Queries;
using Sourcerer.Infrastructure;
using Sourcerer.Infrastructure.Time;
using ThirdDrawer.Extensions.CollectionExtensionMethods;

namespace Sourcerer.DomainConcepts
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IFactStore _factStore;
        private readonly IDomainEventBroker _domainEventBroker;
        private readonly IQueryableSnapshot _snapshot;
        private readonly List<IAggregateRoot> _aggregateRootsInTransaction = new List<IAggregateRoot>();
        private readonly IClock _clock;

        public UnitOfWork(IFactStore factStore, IDomainEventBroker domainEventBroker, IQueryableSnapshot snapshot, IClock clock)
        {
            _factStore = factStore;
            _domainEventBroker = domainEventBroker;
            _snapshot = snapshot;
            _clock = clock;
        }

        public T TryGetEnlistedAggregateRoot<T>(Guid id) where T : IAggregateRoot
        {
            return _aggregateRootsInTransaction
                .OfType<T>()
                .Where(ar => ar.Id == id)
                .FirstOrDefault();
        }

        public void EnlistInTransaction(IAggregateRoot item)
        {
            _aggregateRootsInTransaction.Add(item);
        }

        public void Commit()
        {
            var unitOfWorkId = Guid.NewGuid();
            var sequenceNumber = 0;

            var facts = new List<IFact>();
            while (true)
            {
                var factsFromThisPass = _aggregateRootsInTransaction
                    .SelectMany(item => item.GetAndClearPendingFacts())
                    .ToArray();

                if (factsFromThisPass.None()) break;

                facts.AddRange(factsFromThisPass);
                foreach (var fact in factsFromThisPass)
                {
                    fact.SetUnitOfWorkProperties(new UnitOfWorkProperties(unitOfWorkId, sequenceNumber, _clock.UtcNow));
                    _domainEventBroker.Raise((dynamic) fact);

                    sequenceNumber++;
                }
            }

            var factsArray = facts.ToArray();
            _factStore.AppendAtomically(factsArray);
            _aggregateRootsInTransaction.Do(ar => ar.RevisionId = unitOfWorkId).Done();
            _snapshot.NotifyAggregateRootsModified(_aggregateRootsInTransaction, factsArray);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}