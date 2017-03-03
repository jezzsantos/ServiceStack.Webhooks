using Funq;
using ServiceStack.Configuration;
using ServiceStack.Logging;
using ServiceStack.Validation;

namespace ServiceStack.Webhooks.Azure.IntTests.Services
{
    public class AppHostForAzureTesting : AppSelfHostBase
    {
        public AppHostForAzureTesting()
            : base("AppHostForAzureTesting", typeof(AppHostForAzureTesting).Assembly)
        {
        }

        public override void Configure(Container container)
        {
            LogManager.LogFactory = new ConsoleLogFactory();
            Plugins.Add(new ValidationFeature());

            var settings = new AppSettings();
            container.Register<IAppSettings>(settings);
            container.Register<IWebhookSubscriptionStore>(new AzureTableWebhookSubscriptionStore(settings));
            container.Register<IWebhookEventSink>(new AzureQueueWebhookEventSink(settings));

            Plugins.Add(new WebhookFeature());
        }
    }
}