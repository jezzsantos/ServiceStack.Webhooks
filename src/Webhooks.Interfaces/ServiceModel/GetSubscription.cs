using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.ServiceModel
{
    [Route(Subscription.RootPath + "/subscriptions/{Id}", "GET")]
    public class GetSubscription : IReturn<GetSubscriptionResponse>
    {
        public string Id { get; set; }
    }

    public class GetSubscriptionResponse
    {
        public ResponseStatus ResponseStatus { get; set; }

        public WebhookSubscription Subscription { get; set; }
    }
}