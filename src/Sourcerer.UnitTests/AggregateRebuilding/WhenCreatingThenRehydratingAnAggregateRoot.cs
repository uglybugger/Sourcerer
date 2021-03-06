﻿using System;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using Sourcerer.DomainConcepts;
using Sourcerer.DomainConcepts.Queries;
using Sourcerer.Infrastructure;
using Sourcerer.Infrastructure.Time;
using Sourcerer.Persistence.Memory;
using Sourcerer.SchemaUpgradeTests.v0.Domain.StudentAggregate;

namespace Sourcerer.UnitTests.AggregateRebuilding
{
    [TestFixture]
    public class WhenCreatingThenRehydratingAnAggregateRoot
    {
        [Test]
        public void NothingShouldGoBang()
        {
            var factStore = new MemoryFactStore();
            var aggregateRebuilder = new AggregateRebuilder(factStore);
            var snapshot = new QueryModel<Student>(aggregateRebuilder);
            var eventBroker = Substitute.For<IDomainEventBroker>();

            Guid studentId;

            using (var unitOfWork = new UnitOfWork(factStore, eventBroker, new SystemClock()))
            {
                var repository = new Repository<Student>(snapshot, unitOfWork);

                var student = Student.Create("Fred", "Flintstone");
                studentId = student.Id;
                repository.Add(student);

                unitOfWork.Complete();
            }

            using (var unitOfWork = new UnitOfWork(factStore, eventBroker, new SystemClock()))
            {
                var repository = new Repository<Student>(snapshot, unitOfWork);
                var student = repository.GetById(studentId);

                student.FirstName.ShouldBe("Fred");
                student.LastName.ShouldBe("Flintstone");
            }
        }
    }
}