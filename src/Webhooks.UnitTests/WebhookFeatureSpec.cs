using NUnit.Framework;

namespace ServiceStack.Webhooks.UnitTests
{
    [TestFixture]
    public class WebhookFeatureSpec
    {
        [SetUp]
        public void InitializeContext()
        {
            feature = new WebhookFeature();
        }

        private WebhookFeature feature;

        [Test, Category("Unit")]
        public void WhenCtor_Then()
        {
            Assert.That(true, Is.True);
        }
    }
}