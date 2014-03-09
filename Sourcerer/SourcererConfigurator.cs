using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sourcerer.DomainConcepts.Queries;
using Sourcerer.Infrastructure;
using Sourcerer.Infrastructure.Time;
using Sourcerer.Persistence.Memory;

namespace Sourcerer
{
    public class SourcererConfigurator
    {
        private SourcererConfigurator()
        {
            var assembliesInCallStack = new StackTrace().GetFrames()
                                                        .Select(f => f.GetMethod())
                                                        .Select(m => m.DeclaringType.Assembly)
                                                        .Distinct()
                                                        .ToArray();

            TypesProvider = new AssemblyScanningTypesProvider(assembliesInCallStack);
            FactStore = new MemoryFactStore();
            Clock = new SystemClock();
            DependencyResolver = new DefaultDependencyResolver(assembliesInCallStack);
        }

        public ITypesProvider TypesProvider { get; set; }
        public IDependencyResolver DependencyResolver { get; set; }
        public MemoryFactStore FactStore { get; set; }
        public IClock Clock { get; set; }

        public static SourcererConfigurator Configure()
        {
            return new SourcererConfigurator();
        }

        public void Abracadabra()
        {
            var aggregateRebuilder = new AggregateRebuilder(FactStore);
            var domainEventBroker = new DomainEventBroker(DependencyResolver);
            SourcererFactory.Configure(FactStore, domainEventBroker, Clock);
        }
    }
}