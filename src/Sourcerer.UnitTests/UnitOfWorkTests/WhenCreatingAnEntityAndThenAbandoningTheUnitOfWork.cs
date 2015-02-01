using System;
using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;
using Sourcerer.SchemaUpgradeTests.v0.Domain.StudentAggregate;

namespace Sourcerer.UnitTests.UnitOfWorkTests
{
    [TestFixture]
    public class WhenCreatingAnEntityAndThenAbandoningTheUnitOfWork
    {
        [Test]
        public void ThatEntityShouldNotExist()
        {
            var sourcererFactory= SourcererConfigurator.Configure().Abracadabra();

            Guid fredId;

            using (var unitOfWork = sourcererFactory.CreateUnitOfWork())
            {
                var studentRepository = sourcererFactory.CreateRepository<Student>(unitOfWork);

                var fred = Student.Create("Fred", "Flintstone");
                fredId = fred.Id;
                studentRepository.Add(fred);
            }

            using (var unitOfWork = sourcererFactory.CreateUnitOfWork())
            {
                var studentRepository = sourcererFactory.CreateRepository<Student>(unitOfWork);

                Should.Throw<KeyNotFoundException>(() => studentRepository.GetById(fredId));
            }
        }
    }
}