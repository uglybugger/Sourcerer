using System;
using System.Collections.Concurrent;
using Sourcerer.DomainConcepts;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.DomainConcepts.Queries;
using Sourcerer.Infrastructure;
using Sourcerer.Infrastructure.Time;

namespace Sourcerer
{
    public class SourcererFactory
    {
        private readonly IFactStore _factStore;
        private readonly IDomainEventBroker _domainEventBroker;
        private readonly IClock _clock;
        private readonly ConcurrentDictionary<Type, object> _snapshots = new ConcurrentDictionary<Type, object>();
        private readonly IAggregateRebuilder _aggregateRebuilder;

        internal SourcererFactory(IFactStore factStore, IDomainEventBroker domainEventBroker, IClock clock)
        {
            _factStore = factStore;
            _domainEventBroker = domainEventBroker;
            _clock = clock;
            _aggregateRebuilder = new AggregateRebuilder(factStore); //FIXME extract
        }

        public IUnitOfWork CreateUnitOfWork()
        {
            return new UnitOfWork(_factStore, _domainEventBroker, _clock);
        }

        public IRepository<TAggregateRoot> CreateRepository<TAggregateRoot>(IUnitOfWork unitOfWork) where TAggregateRoot : class, IAggregateRoot
        {
            return new Repository<TAggregateRoot>(GetOrCreateSnapshot<TAggregateRoot>(), unitOfWork);
        }

        public IQueryContext<TAggregateRoot> CreateQueryContext<TAggregateRoot>() where TAggregateRoot : class, IAggregateRoot
        {
            return new QueryContext<TAggregateRoot>(GetOrCreateSnapshot<TAggregateRoot>());
        }

        private IQueryModel<TAggregateRoot> GetOrCreateSnapshot<TAggregateRoot>() where TAggregateRoot : class, IAggregateRoot
        {
            return (IQueryModel<TAggregateRoot>) _snapshots.GetOrAdd(typeof (TAggregateRoot), t => new QueryModel<TAggregateRoot>(_aggregateRebuilder));
        }
    }
}