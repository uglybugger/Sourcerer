using System;
using System.Linq;
using System.Reflection;
using Sourcerer.DomainConcepts.Facts;
using ThirdDrawer.Extensions;

namespace Sourcerer.Infrastructure
{
    public class AssemblyScanningFactTypesProvider : IFactTypesProvider
    {
        private readonly Assembly[] _assembliesToScan;
        private readonly Lazy<Type[]> _factTypes;

        public AssemblyScanningFactTypesProvider(Assembly[] assembliesToScan)
        {
            if (assembliesToScan.None()) throw new ArgumentException("You must provide at least one assembly that contains fact types", "assembliesToScan");

            _factTypes = new Lazy<Type[]>(ScanForFactTypes);
            _assembliesToScan = assembliesToScan;
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
                .Where(t => !t.IsInterface)
                .Where(t => !t.IsAbstract)
                .ToArray();

            return factTypes;
        }
    }
}