using System.Collections.Generic;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.ServiceModel
{
    [Route("/webhooks/subscriptions/search", "GET")]
    public class SearchSubscriptions : IReturn<SearchSubscriptionsResponse>
    {
        public string EventName { get; set; }
    }

    public class SearchSubscriptionsResponse
    {
        public ResponseStatus ResponseStatus { get; set; }

        public List<SubscriptionConfig> Subscribers { get; set; }
    }
}