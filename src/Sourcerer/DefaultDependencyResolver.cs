using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private readonly Lazy<Type[]> _knownTypes;

        public DefaultDependencyResolver(Assembly[] assemblies)
        {
            _assemblies = assemblies;
            _knownTypes = new Lazy<Type[]>(LoadKnownTypes);
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
            return KnownTypes
                .Where(type.IsAssignableFrom)
                .ToArray();
        }

        private IEnumerable<Type> KnownTypes
        {
            get { return _knownTypes.Value; }
        }

        private Type[] LoadKnownTypes()
        {
            return _assemblies
                .SelectMany(a => a.GetExportedTypes())
                .Where(t => t.IsInstantiable())
                .ToArray();
        }
    }
}