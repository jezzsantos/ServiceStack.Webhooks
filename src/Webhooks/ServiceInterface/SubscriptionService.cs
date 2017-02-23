using System;
using System.Linq;
using ServiceStack.Data;
using ServiceStack.Webhooks.Properties;
using ServiceStack.Webhooks.ServiceModel;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.ServiceInterface
{
    internal class SubscriptionService : Service
    {
        public IWebhookSubscriptionStore Store { get; set; }

        public ICurrentCaller Caller
        {
            get { return Request.ToCaller(); }
        }

        public CreateSubscriptionResponse Post(CreateSubscription request)
        {
            var now = DateTime.UtcNow.ToNearestSecond();
            var subscriptions = request.Events.Select(ev => new WebhookSubscription
            {
                Config = request.Config,
                Name = request.Name,
                IsActive = true,
                CreatedDateUtc = now,
                LastModifiedDateUtc = now,
                CreatedById = Caller.UserId,
                Event = ev
            }).ToList();

            subscriptions.ForEach(sub =>
            {
                if (Store.Get(sub.CreatedById, sub.Event) != null)
                {
                    throw new OptimisticConcurrencyException(Resources.SubscriptionService_DuplicateRegistration.Fmt(sub.Event));
                }

                var id = Store.Add(sub);
                sub.Id = id;
            });

            return new CreateSubscriptionResponse
            {
                Subscriptions = subscriptions
            };
        }
    }
}