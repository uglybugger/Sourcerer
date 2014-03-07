namespace Sourcerer.Infrastructure
{
    public interface IHandleFact<T>
    {
        void Handle(T domainEvent);
    }
}