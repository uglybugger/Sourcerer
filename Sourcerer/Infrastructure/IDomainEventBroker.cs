using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.Infrastructure
{
    public interface IDomainEventBroker
    {
        void Raise<T>(T fact) where T : IFact;
    }
}