using System;
using System.Linq;
using System.Reflection;
using ThirdDrawer.Extensions.TypeExtensionMethods;

namespace Sourcerer.Infrastructure.Migrations
{
    public class SchemaMigrator
    {
        private readonly Assembly[] _factAssemblies;
        private readonly Func<Type, bool> _migrationFilterFunc;
        private readonly Func<Type, int> _versionNumberFunc;
        private readonly IFactStoreFactory _factStoreFactory;

        public SchemaMigrator(Assembly[] factAssemblies, Func<Type, bool> migrationFilterFunc, Func<Type, int> versionNumberFunc, IFactStoreFactory factStoreFactory)
        {
            _factAssemblies = factAssemblies;
            _migrationFilterFunc = migrationFilterFunc;
            _versionNumberFunc = versionNumberFunc;
            _factStoreFactory = factStoreFactory;
        }

        public void DoYourThing()
        {
            var factMigratorTypes = _factAssemblies
                .SelectMany(a => a.GetExportedTypes())
                .Where(t => t.IsAssignableTo<IMigrateFact>())
                .Where(t => _migrationFilterFunc(t))
                .ToArray();

            var versionMigrations = factMigratorTypes
                .GroupBy(t => _versionNumberFunc(t))
                .OrderBy(kvp => kvp.Key)
                .ToArray();

            for (var schemaVersion = 1; schemaVersion < versionMigrations.Max(kvp => kvp.Key); schemaVersion++)
            {
                var oldFactStore = _factStoreFactory.GetFactStore(schemaVersion - 1);
                var newFactStore = _factStoreFactory.GetFactStore(schemaVersion);

                if (newFactStore.HasFacts) continue;

                var versionMigrator = new VersionMigrator(versionMigrations[schemaVersion]);
                var oldFacts = oldFactStore.GetAllFactsGroupedByUnitOfWork();
                var migratedFacts = versionMigrator.Migrate(oldFacts);
                newFactStore.ImportFrom(migratedFacts);
            }
        }
    }
}