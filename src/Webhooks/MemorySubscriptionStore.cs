using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks
{
    public class MemorySubscriptionStore : ISubscriptionStore
    {
        private readonly ConcurrentDictionary<string, SubscriptionDeliveryResult> deliveryResults = new ConcurrentDictionary<string, SubscriptionDeliveryResult>();
        private readonly ConcurrentDictionary<string, WebhookSubscription> subscriptions = new ConcurrentDictionary<string, WebhookSubscription>();

        public string Add(WebhookSubscription subscription)
        {
            Guard.AgainstNull(() => subscription, subscription);

            var id = DataFormats.CreateEntityIdentifier();
            subscription.Id = id;

            subscriptions.TryAdd(id, subscription);

            return id;
        }

        public List<WebhookSubscription> Find(string userId)
        {
            return subscriptions
                .Where(kvp => kvp.Value.CreatedById.EqualsIgnoreCase(userId))
                .Select(kvp => kvp.Value)
                .ToList();
        }

        public WebhookSubscription Get(string userId, string eventName)
        {
            Guard.AgainstNullOrEmpty(() => eventName, eventName);

            return subscriptions
                .Where(kvp => kvp.Value.CreatedById.EqualsIgnoreCase(userId)
                              && kvp.Value.Event.EqualsIgnoreCase(eventName))
                .Select(kvp => kvp.Value)
                .FirstOrDefault();
        }

        public WebhookSubscription Get(string subscriptionId)
        {
            Guard.AgainstNullOrEmpty(() => subscriptionId, subscriptionId);

            return subscriptions
                .Where(kvp => kvp.Value.Id.EqualsIgnoreCase(subscriptionId))
                .Select(kvp => kvp.Value)
                .FirstOrDefault();
        }

        public void Update(string subscriptionId, WebhookSubscription subscription)
        {
            Guard.AgainstNullOrEmpty(() => subscriptionId, subscriptionId);
            Guard.AgainstNull(() => subscription, subscription);

            var existing = Get(subscriptionId);
            if (existing != null)
            {
                subscriptions[subscriptionId] = subscription;
            }
        }

        public void Delete(string subscriptionId)
        {
            Guard.AgainstNullOrEmpty(() => subscriptionId, subscriptionId);

            var existing = Get(subscriptionId);
            if (existing != null)
            {
                WebhookSubscription subscription;
                subscriptions.TryRemove(subscriptionId, out subscription);
            }
        }

        public List<SubscriptionRelayConfig> Search(string eventName, bool? isActive)
        {
            Guard.AgainstNullOrEmpty(() => eventName, eventName);

            return subscriptions
                .Where(kvp => kvp.Value.Event.EqualsIgnoreCase(eventName)
                              && (!isActive.HasValue || isActive.Value == kvp.Value.IsActive))
                .Select(kvp => new SubscriptionRelayConfig
                {
                    Config = kvp.Value.Config,
                    SubscriptionId = kvp.Value.Id
                })
                .ToList();
        }

        public void Add(string subscriptionId, SubscriptionDeliveryResult result)
        {
            Guard.AgainstNullOrEmpty(() => subscriptionId, subscriptionId);
            Guard.AgainstNull(() => result, result);

            var subscription = Get(subscriptionId);
            if (subscription != null)
            {
                deliveryResults.TryAdd(result.Id, result);
            }
        }

        public List<SubscriptionDeliveryResult> Search(string subscriptionId, int top)
        {
            Guard.AgainstNullOrEmpty(() => subscriptionId, subscriptionId);
            if (top <= 0)
            {
                throw new ArgumentOutOfRangeException("top");
            }

            return deliveryResults
                .Where(kvp => kvp.Value.SubscriptionId.EqualsIgnoreCase(subscriptionId))
                .Select(kvp => kvp.Value)
                .OrderByDescending(sub => sub.AttemptedDateUtc)
                .Take(top)
                .ToList();
        }

        public void Clear()
        {
            deliveryResults.Clear();
            subscriptions.Clear();
        }
    }
}