using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using ServiceStack.Logging;
using ServiceStack.Webhooks.Properties;
using ServiceStack.Webhooks.Relays;
using ServiceStack.Webhooks.ServiceModel;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.ServiceInterface
{
    internal class SubscriptionService : Service, ISubscriptionService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(SubscriptionService));

        public ISubscriptionStore Store { get; set; }

        public ICurrentCaller Caller
        {
            get { return Request.ToCaller(); }
        }

        List<SubscriptionRelayConfig> ISubscriptionService.Search(string eventName)
        {
            return Store.Search(eventName, true);
        }

        void ISubscriptionService.UpdateResults(List<SubscriptionDeliveryResult> results)
        {
            UpdateDeliveryHistory(results);
        }

        public SearchSubscriptionsResponse Get(SearchSubscriptions request)
        {
            var subscribers = Store.Search(request.EventName, true);

            logger.InfoFormat(@"Searched subscriptions for event {0} by user {1}", request.EventName, Caller.UserId);

            return new SearchSubscriptionsResponse
            {
                Subscribers = subscribers
            };
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

                logger.InfoFormat(@"Created subscription {0} to event {1} by user {2}", sub.Id, sub.Event, Caller.UserId);
            });

            return new CreateSubscriptionResponse
            {
                Subscriptions = subscriptions
            };
        }

        public GetSubscriptionResponse Get(GetSubscription request)
        {
            var subscription = Store.Get(request.Id);
            if (subscription == null)
            {
                throw HttpError.NotFound(null);
            }

            var history = Store.Search(request.Id, 100)
                .OrderByDescending(result => result.AttemptedDateUtc)
                .ToList();

            logger.InfoFormat(@"Retrieved subscription {0} by user {1}", subscription.Id, Caller.UserId);

            return new GetSubscriptionResponse
            {
                Subscription = subscription,
                History = history
            };
        }

        public ListSubscriptionsResponse Get(ListSubscriptions request)
        {
            var subscriptions = Store.Find(Caller.UserId);

            logger.InfoFormat(@"Listed subscription for user {0}", Caller.UserId);

            return new ListSubscriptionsResponse
            {
                Subscriptions = subscriptions
            };
        }

        public UpdateSubscriptionResponse Put(UpdateSubscription request)
        {
            var now = DateTime.UtcNow.ToNearestSecond();
            var subscription = Store.Get(request.Id);
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

            logger.InfoFormat(@"Updated subscription {0} by user {1}", subscription.Id, Caller.UserId);

            return new UpdateSubscriptionResponse
            {
                Subscription = subscription
            };
        }

        public DeleteSubscriptionResponse Delete(DeleteSubscription request)
        {
            var subscription = Store.Get(request.Id);
            if (subscription == null)
            {
                throw HttpError.NotFound(null);
            }

            Store.Delete(subscription.Id);

            logger.InfoFormat(@"Deleted subscription {0} by user {1}", subscription.Id, Caller.UserId);

            return new DeleteSubscriptionResponse();
        }

        public UpdateSubscriptionHistoryResponse Put(UpdateSubscriptionHistory request)
        {
            if (request.Results.Any())
            {
                UpdateDeliveryHistory(request.Results);

                logger.InfoFormat(@"Added subscription history by user {0}", Caller.UserId);
            }

            return new UpdateSubscriptionHistoryResponse();
        }

        private void UpdateDeliveryHistory(List<SubscriptionDeliveryResult> results)
        {
            results.ForEach(incoming =>
            {
                var existing = Store.Search(incoming.SubscriptionId, results.Count);
                if (!existing.Any(exist => exist.Id.EqualsIgnoreCase(incoming.Id)))
                {
                    Store.Add(incoming.SubscriptionId, incoming);

                    if ((incoming.StatusCode >= HttpStatusCode.BadRequest)
                        && (incoming.StatusCode < HttpStatusCode.InternalServerError))
                    {
                        var subscription = Store.Get(incoming.SubscriptionId);
                        subscription.IsActive = false;
                        Store.Update(incoming.SubscriptionId, subscription);
                    }
                }
            });
        }
    }
}