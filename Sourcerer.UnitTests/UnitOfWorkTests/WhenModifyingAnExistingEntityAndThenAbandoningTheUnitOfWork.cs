using System;
using NUnit.Framework;
using Shouldly;
using Sourcerer.SchemaUpgradeTests.v0.Domain.StudentAggregate;

namespace Sourcerer.UnitTests.UnitOfWorkTests
{
    [TestFixture]
    public class WhenModifyingAnExistingEntityAndThenAbandoningTheUnitOfWork
    {
        [Test]
        public void TheChangesShouldNotBePersisted()
        {
            SourcererConfigurator.Configure().Abracadabra();

            Guid fredId;

            using (var unitOfWork = SourcererFactory.CreateUnitOfWork())
            {
                var studentRepository = SourcererFactory.CreateRepository<Student>(unitOfWork);

                var fred = Student.Create("Fred", "Flintstone");
                fredId = fred.Id;
                studentRepository.Add(fred);
                unitOfWork.Complete();
            }

            using (var unitOfWork = SourcererFactory.CreateUnitOfWork())
            {
                var studentRepository = SourcererFactory.CreateRepository<Student>(unitOfWork);
                var fred = studentRepository.GetById(fredId);
                fred.ChangeAddress("123 Bedrock Place", "Bedrock", "FL", "31337");
            }

            using (var unitOfWork = SourcererFactory.CreateUnitOfWork())
            {
                var studentRepository = SourcererFactory.CreateRepository<Student>(unitOfWork);
                var fred = studentRepository.GetById(fredId);
                fred.StreetAddress.ShouldBe(null);
            }
        }
    }
}