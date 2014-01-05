using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.Infrastructure
{
    public interface IEventBroker
    {
        void Raise<T>(T fact) where T : IFact;
    }
}