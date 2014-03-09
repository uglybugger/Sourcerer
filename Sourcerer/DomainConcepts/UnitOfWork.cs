using System;
using System.Collections.Generic;
using System.Linq;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.DomainConcepts.Facts;
using Sourcerer.Infrastructure;
using Sourcerer.Infrastructure.Time;
using ThirdDrawer.Extensions.CollectionExtensionMethods;

namespace Sourcerer.DomainConcepts
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IFactStore _factStore;
        private readonly IDomainEventBroker _domainEventBroker;
        private readonly List<IAggregateRoot> _enlistedItems = new List<IAggregateRoot>();
        private readonly IClock _clock;
        private bool _completed;
        private bool _abandoned;

        public UnitOfWork(IFactStore factStore, IDomainEventBroker domainEventBroker, IClock clock)
        {
            _factStore = factStore;
            _domainEventBroker = domainEventBroker;
            _clock = clock;
        }

        public void Enlist(IAggregateRoot item)
        {
            _enlistedItems.Add(item);
        }

        public EventHandler<EventArgs> Completed { get; set; }

        public void Complete()
        {
            if (_abandoned) throw new InvalidOperationException();

            var unitOfWorkId = Guid.NewGuid();
            var sequenceNumber = 0;

            var facts = new List<IFact>();
            while (true)
            {
                var factsFromThisPass = _enlistedItems
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
            _enlistedItems.Do(ar => ar.RevisionId = unitOfWorkId).Done();

            _completed = true;

            var completedHandler = Completed;
            if (completedHandler == null) return;
            completedHandler(this, EventArgs.Empty);
        }

        public EventHandler<EventArgs> Abandoned { get; set; }

        public void Abandon()
        {
            if (_completed) throw new InvalidOperationException();

            _abandoned = true;

            var abandonedHandler = Abandoned;
            if (abandonedHandler == null) return;
            abandonedHandler(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            if (!_completed) Abandon();
        }
    }
}