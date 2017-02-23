using NUnit.Framework;

namespace ServiceStack.Webhooks.UnitTests
{
    [TestFixture]
    public class WebhookFeatureSpec
    {
        private WebhookFeature _feature;

        [SetUp]
        public void InitializeContext()
        {
            _feature = new WebhookFeature();
        }

        [Test, Category("Unit")]
        public void WhenCtor_Then()
        {
            Assert.That(true, Is.True);
        }
    }
}