using Sourcerer.DomainConcepts.Facts;
using ThirdDrawer.Extensions.CollectionExtensionMethods;

namespace Sourcerer.Infrastructure
{
    internal class DomainEventBroker : IDomainEventBroker
    {
        private readonly IDependencyResolver _dependencyResolver;

        public DomainEventBroker(IDependencyResolver dependencyResolver)
        {
            _dependencyResolver = dependencyResolver;
        }

        public void Raise<T>(T fact) where T : IFact
        {
            OwnedComponent<IHandleFact<T>>[] handlers = null;
            try
            {
                handlers = _dependencyResolver.ResolveAllOwnedComponents<IHandleFact<T>>();
                handlers.Do(h => h.Component.Handle(fact))
                        .Done();
            }
            finally
            {
                if (handlers != null)
                {
                    handlers.Do(h => h.Dispose())
                            .Done();
                }
            }
        }
    }
}