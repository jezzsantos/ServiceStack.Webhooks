using ServiceStack.Model;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.ServiceModel
{
    [Route(Subscription.RootPath + "/subscriptions/{Id}", "PUT")]
    public class UpdateSubscription : IPut, IHasStringId, IReturn<UpdateSubscriptionResponse>
    {
        public string Url { get; set; }

        public string Secret { get; set; }

        public string ContentType { get; set; }

        public bool? IsActive { get; set; }

        public string Id { get; set; }
    }

    public class UpdateSubscriptionResponse
    {
        public ResponseStatus ResponseStatus { get; set; }

        public WebhookSubscription Subscription { get; set; }
    }
}