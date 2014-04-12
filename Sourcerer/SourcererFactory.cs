﻿using System;
using System.Collections.Concurrent;
using Sourcerer.DomainConcepts;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.DomainConcepts.Queries;
using Sourcerer.Infrastructure;
using Sourcerer.Infrastructure.Time;

namespace Sourcerer
{
    //FIXME make non-static.
    public static class SourcererFactory
    {
        private static IFactStore _factStore;
        private static IDomainEventBroker _domainEventBroker;
        private static IClock _clock;
        private static readonly ConcurrentDictionary<Type, object> _snapshots = new ConcurrentDictionary<Type, object>();
        private static IAggregateRebuilder _aggregateRebuilder;

        internal static void Configure(IFactStore factStore, IDomainEventBroker domainEventBroker, IClock clock)
        {
            _factStore = factStore;
            _domainEventBroker = domainEventBroker;
            _clock = clock;
            _aggregateRebuilder = new AggregateRebuilder(factStore); //FIXME extract
        }

        public static IUnitOfWork CreateUnitOfWork()
        {
            return new UnitOfWork(_factStore, _domainEventBroker, _clock);
        }

        public static IRepository<TAggregateRoot> CreateRepository<TAggregateRoot>(IUnitOfWork unitOfWork) where TAggregateRoot : class, IAggregateRoot
        {
            return new Repository<TAggregateRoot>(GetOrCreateSnapshot<TAggregateRoot>(), unitOfWork);
        }

        public static IQueryContext<TAggregateRoot> CreateQueryContext<TAggregateRoot>() where TAggregateRoot : class, IAggregateRoot
        {
            return new QueryContext<TAggregateRoot>(GetOrCreateSnapshot<TAggregateRoot>());
        }

        private static IQueryModel<TAggregateRoot> GetOrCreateSnapshot<TAggregateRoot>() where TAggregateRoot : class, IAggregateRoot
        {
            return (IQueryModel<TAggregateRoot>) _snapshots.GetOrAdd(typeof (TAggregateRoot), t => new QueryModel<TAggregateRoot>(_aggregateRebuilder));
        }
    }
}