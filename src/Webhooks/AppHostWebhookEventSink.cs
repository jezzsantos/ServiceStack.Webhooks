using System;
using ServiceStack.Webhooks.Clients;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks
{
    internal class AppHostWebhookEventSink : IWebhookEventSink
    {
        internal const int DefaultServiceClientRetries = 3;
        internal const int DefaultServiceClientTimeoutSeconds = 60;

        public AppHostWebhookEventSink()
        {
            Retries = DefaultServiceClientRetries;
            TimeoutSecs = DefaultServiceClientTimeoutSeconds;
        }

        public IWebhookEventSubscriptionCache SubscriptionCache { get; set; }

        public IWebhookEventServiceClient ServiceClient { get; set; }

        public int Retries { get; set; }

        public int TimeoutSecs { get; set; }

        public void Write<TDto>(string eventName, TDto data)
        {
            Guard.AgainstNullOrEmpty(() => eventName, eventName);

            var subscriptions = SubscriptionCache.GetAll(eventName);
            subscriptions.ForEach(sub =>
                    NotifySubscription(sub, eventName, data));
        }

        private void NotifySubscription<TDto>(SubscriptionConfig subscription, string eventName, TDto data)
        {
            ServiceClient.Retries = Retries;
            ServiceClient.Timeout = TimeSpan.FromSeconds(TimeoutSecs);
            ServiceClient.Post(subscription, eventName, data);
        }
    }
}