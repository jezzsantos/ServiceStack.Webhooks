using System.Collections.Generic;
using ServiceStack.Model;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.ServiceModel
{
    [Route("/webhooks/subscriptions/{Id}", "GET")]
    public class GetSubscription : IGet, IHasStringId, IReturn<GetSubscriptionResponse>
    {
        public string Id { get; set; }
    }

    public class GetSubscriptionResponse
    {
        public ResponseStatus ResponseStatus { get; set; }

        public WebhookSubscription Subscription { get; set; }

        public List<SubscriptionDeliveryResult> History { get; set; }
    }
}