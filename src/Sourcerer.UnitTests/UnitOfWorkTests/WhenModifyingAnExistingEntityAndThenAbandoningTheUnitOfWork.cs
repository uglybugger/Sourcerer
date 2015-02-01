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
            var sourcererFactory = SourcererConfigurator.Configure().Abracadabra();

            Guid fredId;

            using (var unitOfWork = sourcererFactory.CreateUnitOfWork())
            {
                var studentRepository = sourcererFactory.CreateRepository<Student>(unitOfWork);

                var fred = Student.Create("Fred", "Flintstone");
                fredId = fred.Id;
                studentRepository.Add(fred);
                unitOfWork.Complete();
            }

            using (var unitOfWork = sourcererFactory.CreateUnitOfWork())
            {
                var studentRepository = sourcererFactory.CreateRepository<Student>(unitOfWork);
                var fred = studentRepository.GetById(fredId);
                fred.ChangeAddress("123 Bedrock Place", "Bedrock", "FL", "31337");
            }

            using (var unitOfWork = sourcererFactory.CreateUnitOfWork())
            {
                var studentRepository = sourcererFactory.CreateRepository<Student>(unitOfWork);
                var fred = studentRepository.GetById(fredId);
                fred.StreetAddress.ShouldBe(null);
            }
        }
    }
}