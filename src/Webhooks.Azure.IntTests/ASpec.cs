using NUnit.Framework;

namespace ServiceStack.Webhooks.Azure.IntTests
{
    [TestFixture]
    public class ASpec
    {
        [SetUp]
        public void InitializeContext()
        {
        }

        [Test]
        [Category("Integration")]
        public void When_Then()
        {
            Assert.That(true, Is.True);
        }
    }
}