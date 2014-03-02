using System;
using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.SchemaUpgradeTests.v2.Domain.StudentAggregate.Facts
{
    public class StudentChangedAddressFact : FactAbout<Student>
    {
        public Guid AddressId { get; set; }
    }
}