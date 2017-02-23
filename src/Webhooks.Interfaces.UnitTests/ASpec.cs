using NUnit.Framework;

namespace ServiceStack.Webhooks.Interfaces.UnitTests
{
    [TestFixture]
    public class ASpec
    {
        [SetUp]
        public void InitializeContext()
        {
        }

        [Test, Category("Unit")]
        public void When_Then()
        {
            Assert.That(true, Is.True);
        }
    }
}