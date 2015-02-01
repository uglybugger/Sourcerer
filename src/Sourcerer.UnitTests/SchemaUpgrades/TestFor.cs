using NUnit.Framework;

namespace Sourcerer.UnitTests.SchemaUpgrades
{
    [TestFixture]
    public abstract class TestFor<T>
    {
        protected abstract void Given();
        protected abstract void When();

        [SetUp]
        public void SetUp()
        {
            Given();
            When();
        }
    }
}