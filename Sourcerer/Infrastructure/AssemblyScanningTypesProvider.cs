using System;
using System.Linq;
using System.Reflection;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.DomainConcepts.Facts;
using ThirdDrawer.Extensions.CollectionExtensionMethods;
using ThirdDrawer.Extensions.TypeExtensionMethods;

namespace Sourcerer.Infrastructure
{
    public class AssemblyScanningTypesProvider : ITypesProvider
    {
        private readonly Assembly[] _assembliesToScan;
        private readonly Lazy<Type[]> _factTypes;
        private readonly Lazy<Type[]> _aggregateTypes;

        public AssemblyScanningTypesProvider(Assembly[] assembliesToScan)
        {
            if (assembliesToScan.None()) throw new ArgumentException("You must provide at least one assembly that contains fact types", "assembliesToScan");

            _factTypes = new Lazy<Type[]>(ScanForFactTypes);
            _aggregateTypes = new Lazy<Type[]>(ScanForAggregateTypes);
            _assembliesToScan = assembliesToScan;
        }

        public Type[] AggregateTypes
        {
            get { return _aggregateTypes.Value; }
        }

        private Type[] ScanForAggregateTypes()
        {
            var aggregateTypes = _assembliesToScan
                .SelectMany(a => a.GetExportedTypes())
                .Where(t => t.IsAssignableTo<IAggregateRoot>())
                .Where(t => t.IsInstantiable())
                .ToArray();
            return aggregateTypes;
        }

        public Type[] FactTypes
        {
            get { return _factTypes.Value; }
        }

        private Type[] ScanForFactTypes()
        {
            var factTypes = _assembliesToScan
                .SelectMany(a => a.GetExportedTypes())
                .Where(t => t.IsAssignableTo<IFact>())
                .Where(t => t.IsInstantiable())
                .ToArray();

            return factTypes;
        }
    }
}