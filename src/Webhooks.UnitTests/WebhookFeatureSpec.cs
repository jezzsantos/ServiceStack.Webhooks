using NUnit.Framework;

namespace ServiceStack.Webhooks.UnitTests
{
    [TestFixture]
    public class WebhookFeatureSpec
    {
        private WebhookFeature feature;

        [SetUp]
        public void InitializeContext()
        {
            feature = new WebhookFeature();
        }

        [Test, Category("Unit")]
        public void WhenCtor_Then()
        {
            Assert.That(true, Is.True);
        }
    }
}