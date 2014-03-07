using Sourcerer.DomainConcepts;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.DomainConcepts.Queries;
using Sourcerer.Infrastructure;
using Sourcerer.Infrastructure.Time;

namespace Sourcerer
{
    public static class SourcererFactory
    {
        private static IFactStore _factStore;
        private static IDomainEventBroker _domainEventBroker;
        private static IAggregateRebuilder _aggregateRebuilder;
        private static IQueryableSnapshot _queryableSnapshot;
        private static IClock _clock;

        internal static void Configure(IFactStore factStore,
                                       IDomainEventBroker domainEventBroker,
                                       IAggregateRebuilder aggregateRebuilder,
                                       IQueryableSnapshot queryableSnapshot,
                                       IClock clock)
        {
            _factStore = factStore;
            _domainEventBroker = domainEventBroker;
            _aggregateRebuilder = aggregateRebuilder;
            _queryableSnapshot = queryableSnapshot;
            _clock = clock;
        }

        public static IUnitOfWork CreateUnitOfWork()
        {
            return new UnitOfWork(_factStore, _domainEventBroker, _queryableSnapshot, _clock);
        }

        public static IRepository<TAggregateRoot> CreateRepository<TAggregateRoot>(IUnitOfWork unitOfWork) where TAggregateRoot : class, IAggregateRoot
        {
            return new Repository<TAggregateRoot>(unitOfWork, _queryableSnapshot);
        }
    }
}