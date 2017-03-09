using System;
using System.Collections.Generic;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using ServiceStack.Webhooks;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace WebHooks.OrmLite
{
    public class OrmLiteSubscriptionStore : ISubscriptionStore, IRequiresSchema
    {
        readonly IDbConnectionFactory dbFactory;

        public OrmLiteSubscriptionStore(IDbConnectionFactory dbFactory)
        {
            this.dbFactory = dbFactory;
        }

        public string Add(WebhookSubscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));

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
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentNullException(nameof(eventName));

            using (var db = dbFactory.Open())
            {
                return db.Single<WebhookSubscription>(x => x.CreatedById == userId && x.Event == eventName);
            }
        }

        public WebhookSubscription Get(string subscriptionId)
        {
            if (string.IsNullOrEmpty(subscriptionId))
                throw new ArgumentNullException(nameof(subscriptionId));

            using (var db = dbFactory.Open())
            {
                return db.Single<WebhookSubscription>(x => x.Id == subscriptionId);
            }
        }

        public void Update(string subscriptionId, WebhookSubscription subscription)
        {
            if (string.IsNullOrEmpty(subscriptionId))
                throw new ArgumentNullException(nameof(subscriptionId));
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));

            using (var db = dbFactory.Open())
            {
                db.Update(subscription);
            }
        }

        public void Delete(string subscriptionId)
        {
            if (string.IsNullOrEmpty(subscriptionId))
                throw new ArgumentNullException(nameof(subscriptionId));

            using (var db = dbFactory.Open())
            {
                db.Delete<WebhookSubscription>(x => x.Id == subscriptionId);
            }
        }

        public List<SubscriptionRelayConfig> Search(string eventName, bool? isActive)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentNullException(nameof(eventName));

            using (var db = dbFactory.Open())
            {
                var q = db.From<WebhookSubscription>()
                    .Where(x => x.Event == eventName);

                if (isActive != null)
                {
                    q.And(x => x.IsActive == isActive.Value);
                }

                return db.Select<SubscriptionRelayConfig>(
                    q.Select(x => new { x.Config, SubscriptionId = x.Id }));
            }
        }

        public void Add(string subscriptionId, SubscriptionDeliveryResult result)
        {
            if (string.IsNullOrEmpty(subscriptionId))
                throw new ArgumentNullException(nameof(subscriptionId));
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            using (var db = dbFactory.Open())
            {
                db.Insert(result);
            }
        }

        public List<SubscriptionDeliveryResult> Search(string subscriptionId, int top)
        {
            if (string.IsNullOrEmpty(subscriptionId))
                throw new ArgumentNullException(nameof(subscriptionId));
            if (top <= 0)
                throw new ArgumentOutOfRangeException(nameof(top));

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

        public void InitSchema()
        {
            using (var db = dbFactory.Open())
            {
                db.CreateTableIfNotExists<WebhookSubscription>();
                db.CreateTableIfNotExists<SubscriptionDeliveryResult>();
            }
        }
    }
}
