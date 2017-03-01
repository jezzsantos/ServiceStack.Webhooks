using Funq;
using ServiceStack.Logging;
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
            Licensing.LicenseServiceStackRuntime();
            LogManager.LogFactory = new ConsoleLogFactory();
            Plugins.Add(new ValidationFeature());

            container.RegisterAutoWiredAs<AzureTableWebhookSubscriptionStore, IWebhookSubscriptionStore>();
            container.RegisterAutoWiredAs<AzureQueueWebhookEventSink, IWebhookEventSink>();

            Plugins.Add(new WebhookFeature());
        }
    }
}