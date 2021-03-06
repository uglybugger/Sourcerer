﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sourcerer.DomainConcepts;
using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.Infrastructure.Migrations
{
    public class VersionMigrator
    {
        private readonly Type[] _factMigrators;

        public VersionMigrator(IEnumerable<Type> factMigrators )
        {
            _factMigrators = factMigrators.ToArray();
        }

        public IEnumerable<IFact> Migrate(IEnumerable<IGrouping<Guid, IFact>> sourceUnitsOfWork)
        {
            var context = new MigrationContext();

            foreach (var unitOfWork in sourceUnitsOfWork)
            {
                var sequenceNumber = 0;

                foreach (var sourceFact in unitOfWork)
                {
                    var factMigrator = (IMigrateFact) CreateFactMigrator((dynamic) sourceFact);

                    var resultingFacts = factMigrator == null
                        ? new[] {sourceFact}
                        : factMigrator.Migrate(sourceFact, context);

                    foreach (var resultingFact in resultingFacts)
                    {
                        var unitOfWorkProperties = new UnitOfWorkProperties
                                                   {
                                                       FactTimestamp = sourceFact.UnitOfWorkProperties.FactTimestamp,
                                                       UnitOfWorkId = sourceFact.UnitOfWorkProperties.UnitOfWorkId,
                                                       SequenceNumber = sequenceNumber,
                                                   };
                        resultingFact.SetUnitOfWorkProperties(unitOfWorkProperties);

                        context.Append(resultingFact);
                        sequenceNumber++;
                    }
                }
            }

            return context.Facts;
        }

        private IMigrateFact CreateFactMigrator<TSourceFact>(TSourceFact sourceFact)
        {
            var factMigratorClosedGenericType = typeof (IMigrateFact<TSourceFact>);

            var factMigratorType = _factMigrators
                .Where(factMigratorClosedGenericType.IsAssignableFrom)
                .FirstOrDefault();

            if (factMigratorType == null) return null;

            var factMigrator = Activator.CreateInstance(factMigratorType);
            return (IMigrateFact) factMigrator;
        }
    }
}