using System;
using System.Collections.Generic;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.OrmLite
{
    public class OrmLiteSubscriptionStore : ISubscriptionStore, IRequiresSchema
    {
        private readonly IDbConnectionFactory dbFactory;

        public OrmLiteSubscriptionStore(IDbConnectionFactory dbFactory)
        {
            this.dbFactory = dbFactory;
        }

        public void InitSchema()
        {
            using (var db = dbFactory.Open())
            {
                db.CreateTableIfNotExists<WebhookSubscription>();
                db.CreateTableIfNotExists<SubscriptionDeliveryResult>();
            }
        }

        public string Add(WebhookSubscription subscription)
        {
            Guard.AgainstNull(() => subscription, subscription);

            var id = DataFormats.CreateEntityIdentifier();
            subscription.Id = id;

            using (var db = dbFactory.Open())
            {
                db.Insert(subscription);
            }

            return subscription.Id;
        }

        public List<WebhookSubscription> Find(string userId)
        {
            using (var db = dbFactory.Open())
            {
                return db.Select<WebhookSubscription>(x => x.CreatedById == userId);
            }
        }

        public WebhookSubscription Get(string userId, string eventName)
        {
            Guard.AgainstNullOrEmpty(() => eventName, eventName);

            using (var db = dbFactory.Open())
            {
                return db.Single<WebhookSubscription>(x => x.CreatedById == userId && x.Event == eventName);
            }
        }

        public WebhookSubscription Get(string subscriptionId)
        {
            Guard.AgainstNullOrEmpty(() => subscriptionId, subscriptionId);

            using (var db = dbFactory.Open())
            {
                return db.Single<WebhookSubscription>(x => x.Id == subscriptionId);
            }
        }

        public void Update(string subscriptionId, WebhookSubscription subscription)
        {
            Guard.AgainstNullOrEmpty(() => subscriptionId, subscriptionId);
            Guard.AgainstNull(() => subscription, subscription);

            using (var db = dbFactory.Open())
            {
                db.Update(subscription);
            }
        }

        public void Delete(string subscriptionId)
        {
            Guard.AgainstNullOrEmpty(() => subscriptionId, subscriptionId);

            using (var db = dbFactory.Open())
            {
                db.Delete<WebhookSubscription>(x => x.Id == subscriptionId);
            }
        }

        public List<SubscriptionRelayConfig> Search(string eventName, bool? isActive)
        {
            Guard.AgainstNullOrEmpty(() => eventName, eventName);

            using (var db = dbFactory.Open())
            {
                var q = db.From<WebhookSubscription>()
                    .Where(x => x.Event == eventName);

                if (isActive != null)
                {
                    q.And(x => x.IsActive == isActive.Value);
                }

                return db.Select<SubscriptionRelayConfig>(
                    q.Select(x => new {x.Config, SubscriptionId = x.Id}));
            }
        }

        public void Add(string subscriptionId, SubscriptionDeliveryResult result)
        {
            Guard.AgainstNullOrEmpty(() => subscriptionId, subscriptionId);
            Guard.AgainstNull(() => result, result);

            using (var db = dbFactory.Open())
            {
                db.Insert(result);
            }
        }

        public List<SubscriptionDeliveryResult> Search(string subscriptionId, int top)
        {
            Guard.AgainstNullOrEmpty(() => subscriptionId, subscriptionId);
            if (top <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(top));
            }

            using (var db = dbFactory.Open())
            {
                return db.Select(db.From<SubscriptionDeliveryResult>()
                    .Where(x => x.SubscriptionId == subscriptionId)
                    .OrderByDescending(x => x.AttemptedDateUtc)
                    .Take(top));
            }
        }

        public void Clear()
        {
            using (var db = dbFactory.Open())
            {
                db.DeleteAll<SubscriptionDeliveryResult>();
                db.DeleteAll<WebhookSubscription>();
            }
        }
    }
}