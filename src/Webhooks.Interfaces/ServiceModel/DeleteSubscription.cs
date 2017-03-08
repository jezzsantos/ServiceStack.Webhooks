using ServiceStack.Model;

namespace ServiceStack.Webhooks.ServiceModel
{
    [Route(Subscription.RootPath + "/subscriptions/{Id}", "DELETE")]
    public class DeleteSubscription : IDelete, IHasStringId, IReturn<DeleteSubscriptionResponse>
    {
        public string Id { get; set; }
    }

    public class DeleteSubscriptionResponse
    {
        public ResponseStatus ResponseStatus { get; set; }
    }
}