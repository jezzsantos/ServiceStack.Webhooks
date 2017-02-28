using Funq;
using ServiceStack.Validation;
using ServiceStack.Webhooks.ServiceInterface;
using ServiceStack.Webhooks.ServiceModel;

namespace ServiceStack.Webhooks
{
    public class WebhookFeature : IPlugin
    {
        public void Register(IAppHost appHost)
        {
            var container = appHost.GetContainer();

            RegisterSubscriptionStore(container);
            RegisterSubscriptionService(appHost);
            RegisterClient(container);
        }

        private static void RegisterClient(Container container)
        {
            if (!container.Exists<IWebhookEventSink>())
            {
                container.RegisterAutoWiredAs<AppHostWebhookEventSink, IWebhookEventSink>();
            }
            container.RegisterAutoWiredAs<WebhooksClient, IWebhooks>();
        }

        private static void RegisterSubscriptionStore(Container container)
        {
            if (!container.Exists<IWebhookSubscriptionStore>())
            {
                container.RegisterAutoWiredAs<MemoryWebhookSubscriptionStore, IWebhookSubscriptionStore>();
            }
        }

        private static void RegisterSubscriptionService(IAppHost appHost)
        {
            var container = appHost.GetContainer();
            appHost.RegisterService(typeof(SubscriptionService));

            container.RegisterValidators(typeof(WebHookInterfaces).Assembly, typeof(SubscriptionService).Assembly);
            container.RegisterAutoWiredAs<SubscriptionEventsValidator, ISubscriptionEventsValidator>();
            container.RegisterAutoWiredAs<SubscriptionConfigValidator, ISubscriptionConfigValidator>();

            container.RegisterAutoWiredAs<AuthSessionCurrentCaller, ICurrentCaller>();
        }
    }
}