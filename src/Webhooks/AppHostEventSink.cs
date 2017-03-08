using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.Webhooks.Clients;
using ServiceStack.Webhooks.Relays;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks
{
    internal class AppHostEventSink : IEventSink
    {
        internal const int DefaultServiceClientRetries = 3;
        internal const int DefaultServiceClientTimeoutSeconds = 60;

        public AppHostEventSink()
        {
            Retries = DefaultServiceClientRetries;
            TimeoutSecs = DefaultServiceClientTimeoutSeconds;
        }

        public IEventSubscriptionCache SubscriptionCache { get; set; }

        public IEventServiceClient ServiceClient { get; set; }

        public ISubscriptionService SubscriptionService { get; set; }

        public int Retries { get; set; }

        public int TimeoutSecs { get; set; }

        public void Write(string eventName, Dictionary<string, string> data)
        {
            Guard.AgainstNullOrEmpty(() => eventName, eventName);

            var subscriptions = SubscriptionCache.GetAll(eventName);
            var results = new List<SubscriptionDeliveryResult>();
            subscriptions.ForEach(sub =>
            {
                var result = NotifySubscription(sub, eventName, data);
                if (result != null)
                {
                    results.Add(result);
                }
            });

            if (results.Any())
            {
                SubscriptionService.UpdateResults(results);
            }
        }

        private SubscriptionDeliveryResult NotifySubscription(SubscriptionRelayConfig subscription, string eventName, Dictionary<string, string> data)
        {
            ServiceClient.Retries = Retries;
            ServiceClient.Timeout = TimeSpan.FromSeconds(TimeoutSecs);
            return ServiceClient.Relay(subscription, eventName, data);
        }
    }
}