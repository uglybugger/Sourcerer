using System;
using System.Collections.Generic;
using System.Linq;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.DomainConcepts.Facts;
using Sourcerer.DomainConcepts.Queries;
using Sourcerer.Infrastructure;
using Sourcerer.Infrastructure.Time;
using ThirdDrawer.Extensions;

namespace Sourcerer.DomainConcepts
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IFactStore _factStore;
        private readonly IEventBroker _eventBroker;
        private readonly IQueryableSnapshot _snapshot;
        private readonly List<IAggregateRoot> _aggregateRootsInTransaction = new List<IAggregateRoot>();
        private readonly IClock _clock;

        public UnitOfWork(IFactStore factStore, IEventBroker eventBroker, IQueryableSnapshot snapshot, IClock clock)
        {
            _factStore = factStore;
            _eventBroker = eventBroker;
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

            var facts = new List<IFact>();
            while (true)
            {
                var factsFromThisPass = _aggregateRootsInTransaction
                    .SelectMany(item => item.GetAndClearPendingFacts())
                    .ToArray();

                if (factsFromThisPass.None()) break;

                facts.AddRange(factsFromThisPass);
                var sequenceNumber = 0;
                foreach (var fact in factsFromThisPass)
                {
                    fact.SetUnitOfWorkProperties(new UnitOfWorkProperties(unitOfWorkId, sequenceNumber, _clock.UtcNow));
                    _eventBroker.Raise((dynamic) fact);

                    sequenceNumber++;
                }
            }

            _factStore.AppendAtomically(facts.ToArray());
            _snapshot.NotifyFactsWereCommitted(facts); //FIXME I don't like this at all.
        }

        public void Dispose()
        {
        }
    }
}