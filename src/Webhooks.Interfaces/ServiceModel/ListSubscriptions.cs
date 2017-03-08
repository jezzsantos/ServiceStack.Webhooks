using System.Collections.Generic;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.ServiceModel
{
    [Route(Subscription.RootPath + "/subscriptions", "GET")]
    public class ListSubscriptions : IGet, IReturn<ListSubscriptionsResponse>
    {
    }

    public class ListSubscriptionsResponse
    {
        public ResponseStatus ResponseStatus { get; set; }

        public List<WebhookSubscription> Subscriptions { get; set; }
    }
}