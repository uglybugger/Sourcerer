namespace Sourcerer.Infrastructure
{
    public interface IDependencyResolver
    {
        OwnedComponent<T>[] ResolveAllOwnedComponents<T>();
    }
}