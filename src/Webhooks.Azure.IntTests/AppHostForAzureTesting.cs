using Funq;
using ServiceStack.Validation;

namespace ServiceStack.Webhooks.Azure.IntTests
{
    public class AppHostForAzureTesting : AppSelfHostBase
    {
        public AppHostForAzureTesting()
            : base("AppHostForAzureTesting", typeof(AppHostForAzureTesting).Assembly)
        {
        }

        public override void Configure(Container container)
        {
            Plugins.Add(new ValidationFeature());

            container.RegisterAutoWiredAs<AzureTableWebhookSubscriptionStore, IWebhookSubscriptionStore>();
            container.RegisterAutoWiredAs<AzureQueueWebhookEventStore, IWebhookEventStore>();

            Plugins.Add(new WebhookFeature());
        }
    }
}