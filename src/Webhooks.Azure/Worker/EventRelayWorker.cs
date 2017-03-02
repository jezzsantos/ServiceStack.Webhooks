using Funq;
using ServiceStack.Caching;
using ServiceStack.Configuration;
using ServiceStack.Webhooks.Clients;
using ServiceStack.Webhooks.Relays;
using ServiceStack.Webhooks.Relays.Clients;

namespace ServiceStack.Webhooks.Azure.Worker
{
    /// <summary>
    ///     Provides a worker for relaying webhook events from queue to subscribers
    /// </summary>
    public class EventRelayWorker : WorkerEntryPoint<EventRelayQueueProcessor>
    {
        public EventRelayWorker(Container container)
        {
            Guard.AgainstNull(() => container, container);

            RegisterDependencies(container);

            Processor = container.Resolve<EventRelayQueueProcessor>();
        }

        private static void RegisterDependencies(Container container)
        {
            var appSettings = container.Resolve<IAppSettings>();

            container.RegisterAutoWiredAs<MemoryCacheClient, ICacheClient>();
            container.RegisterAutoWiredAs<EventServiceClientFactory, IEventServiceClientFactory>();
            container.Register<ISubscriptionService>(x => new SubscriptionServiceClient(appSettings)
            {
                ServiceClientFactory = x.Resolve<IEventServiceClientFactory>()
            });
            container.RegisterAutoWiredAs<CacheClientEventSubscriptionCache, IWebhookEventSubscriptionCache>();
            container.RegisterAutoWiredAs<EventServiceClient, IWebhookEventServiceClient>();
            container.Register(x => new EventRelayQueueProcessor(appSettings)
            {
                ServiceClient = x.Resolve<IWebhookEventServiceClient>(),
                SubscriptionCache = x.Resolve<IWebhookEventSubscriptionCache>()
            });
        }
    }
}