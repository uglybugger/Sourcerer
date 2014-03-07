using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Sourcerer.Infrastructure;
using ThirdDrawer.Extensions.TypeExtensionMethods;

namespace Sourcerer
{
    internal class DefaultDependencyResolver : IDependencyResolver
    {
        private readonly Assembly[] _assemblies;
        private readonly ConcurrentDictionary<Type, Type[]> _componentTypes = new ConcurrentDictionary<Type, Type[]>();

        public DefaultDependencyResolver(Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }

        public OwnedComponent<T>[] ResolveAllOwnedComponents<T>()
        {
            var typesToInstantiate = _componentTypes.GetOrAdd(typeof (T), ScanForComponents);
            var results = typesToInstantiate
                .Select(t => (T) Activator.CreateInstance(t))
                .Select(instance => new OwnedComponent<T>(instance))
                .ToArray();

            return results;
        }

        private Type[] ScanForComponents(Type type)
        {
            return _assemblies
                .SelectMany(a => a.GetExportedTypes())
                .Where(type.IsAssignableFrom)
                .Where(t => t.IsInstantiable())
                .ToArray();
        }
    }
}