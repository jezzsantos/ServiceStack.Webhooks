using System.Collections.Generic;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.ServiceModel
{
    [Route(Subscription.RootPath + "/subscriptions", "POST")]
    public class CreateSubscription : IPost, IReturn<CreateSubscriptionResponse>
    {
        public string Name { get; set; }

        public List<string> Events { get; set; }

        public SubscriptionConfig Config { get; set; }
    }

    public class CreateSubscriptionResponse
    {
        public ResponseStatus ResponseStatus { get; set; }

        public List<WebhookSubscription> Subscriptions { get; set; }
    }
}