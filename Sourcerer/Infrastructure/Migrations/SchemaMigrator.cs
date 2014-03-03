﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sourcerer.DomainConcepts;
using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.Infrastructure.Migrations
{
    public class SchemaMigrator
    {
        private readonly Assembly[] _factAssemblies;

        public SchemaMigrator(Assembly[] factAssemblies)
        {
            _factAssemblies = factAssemblies;
        }

        public IEnumerable<IFact> Migrate(IEnumerable<IGrouping<Guid, IFact>> sourceUnitsOfWork)
        {
            foreach (var unitOfWork in sourceUnitsOfWork)
            {
                var sequenceNumber = 0;

                foreach (var sourceFact in unitOfWork)
                {
                    var factMigrator = (IMigrateFact) CreateFactMigrator((dynamic) sourceFact);

                    var resultingFacts = factMigrator == null
                        ? new[] {sourceFact}
                        : factMigrator.Migrate(sourceFact);

                    foreach (var resultingFact in resultingFacts)
                    {
                        var unitOfWorkProperties = new UnitOfWorkProperties
                                                   {
                                                       FactTimestamp = sourceFact.UnitOfWorkProperties.FactTimestamp,
                                                       UnitOfWorkId = sourceFact.UnitOfWorkProperties.UnitOfWorkId,
                                                       SequenceNumber = sequenceNumber,
                                                   };
                        resultingFact.SetUnitOfWorkProperties(unitOfWorkProperties);
                        
                        yield return resultingFact;
                        sequenceNumber++;
                    }
                }
            }
        }

        private IMigrateFact CreateFactMigrator<TSourceFact>(TSourceFact sourceFact)
        {
            var factMigratorClosedGenericType = typeof (IMigrateFact<TSourceFact>);

            var factMigratorType = _factAssemblies
                .SelectMany(a => a.GetExportedTypes())
                .Where(factMigratorClosedGenericType.IsAssignableFrom)
                .FirstOrDefault();

            if (factMigratorType == null) return null;

            var factMigrator = Activator.CreateInstance(factMigratorType);
            return (IMigrateFact) factMigrator;
        }
    }
}