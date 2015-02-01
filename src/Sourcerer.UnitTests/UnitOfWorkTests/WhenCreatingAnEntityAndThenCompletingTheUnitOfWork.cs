using System;
using NUnit.Framework;
using Shouldly;
using Sourcerer.SchemaUpgradeTests.v0.Domain.StudentAggregate;

namespace Sourcerer.UnitTests.UnitOfWorkTests
{
    [TestFixture]
    public class WhenCreatingAnEntityAndThenCompletingTheUnitOfWork
    {
        [Test]
        public void ThatEntityShouldExist()
        {
            var sourcerFactory = SourcererConfigurator.Configure().Abracadabra();

            Guid fredId;

            using (var unitOfWork = sourcerFactory.CreateUnitOfWork())
            {
                var studentRepository = sourcerFactory.CreateRepository<Student>(unitOfWork);

                var fred = Student.Create("Fred", "Flintstone");
                fredId = fred.Id;
                studentRepository.Add(fred);

                unitOfWork.Complete();
            }

            using (var unitOfWork = sourcerFactory.CreateUnitOfWork())
            {
                var studentRepository = sourcerFactory.CreateRepository<Student>(unitOfWork);
                var fred = studentRepository.GetById(fredId);
                fred.ShouldNotBe(null);
            }
        }
    }
}