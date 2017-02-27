using System;
using System.Linq;
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
                    throw HttpError.Conflict(Resources.SubscriptionService_DuplicateRegistration.Fmt(sub.Event));
                }

                var id = Store.Add(sub);
                sub.Id = id;
            });

            return new CreateSubscriptionResponse
            {
                Subscriptions = subscriptions
            };
        }

        public GetSubscriptionResponse Get(GetSubscription request)
        {
            var subscription = Store.Find(Caller.UserId)
                .FirstOrDefault(sub => sub.Id.EqualsIgnoreCase(request.Id));
            if (subscription == null)
            {
                throw HttpError.NotFound(null);
            }

            return new GetSubscriptionResponse
            {
                Subscription = subscription
            };
        }

        public ListSubscriptionsResponse Get(ListSubscriptions request)
        {
            var subscriptions = Store.Find(Caller.UserId);

            return new ListSubscriptionsResponse
            {
                Subscriptions = subscriptions
            };
        }

        public UpdateSubscriptionResponse Put(UpdateSubscription request)
        {
            var now = DateTime.UtcNow.ToNearestSecond();
            var subscription = Store.Find(Caller.UserId)
                .FirstOrDefault(sub => sub.Id.EqualsIgnoreCase(request.Id));
            if (subscription == null)
            {
                throw HttpError.NotFound(null);
            }

            if (request.Url.HasValue()
                && request.Url.NotEqualsIgnoreCase(subscription.Config.Url))
            {
                subscription.Config.Url = request.Url;
            }
            if (request.Secret.HasValue()
                && request.Secret.NotEqualsIgnoreCase(subscription.Config.Secret))
            {
                subscription.Config.Secret = request.Secret;
            }
            if (request.ContentType.HasValue()
                && request.ContentType.NotEqualsIgnoreCase(subscription.Config.ContentType))
            {
                subscription.Config.ContentType = request.ContentType;
            }
            if (request.IsActive.HasValue
                && (request.IsActive.Value != subscription.IsActive))
            {
                subscription.IsActive = request.IsActive.Value;
            }
            subscription.LastModifiedDateUtc = now;

            Store.Update(request.Id, subscription);

            return new UpdateSubscriptionResponse
            {
                Subscription = subscription
            };
        }

        public DeleteSubscriptionResponse Delete(DeleteSubscription request)
        {
            var subscription = Store.Find(Caller.UserId)
                .FirstOrDefault(sub => sub.Id.EqualsIgnoreCase(request.Id));
            if (subscription == null)
            {
                throw HttpError.NotFound(null);
            }

            Store.Delete(subscription.Id);

            return new DeleteSubscriptionResponse();
        }
    }
}