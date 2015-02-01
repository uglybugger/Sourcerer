using System;
using Sourcerer.DomainConcepts;
using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.SchemaUpgradeTests.v2.Domain.StudentAggregate.Facts
{
    [Fact("{03288B6E-CFB8-4BD0-9C21-A28D6A1991B3}")]
    public class StudentChangedAddressFact : FactAbout<Student>
    {
        public Guid AddressId { get; set; }
    }
}