using System.Collections.Generic;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.ServiceModel
{
    [Route(Subscription.RootPath + "/subscriptions/history", "PUT")]
    public class UpdateSubscriptionHistory : IPut, IReturn<UpdateSubscriptionHistoryResponse>
    {
        public List<SubscriptionDeliveryResult> Results { get; set; }
    }

    public class UpdateSubscriptionHistoryResponse
    {
        public ResponseStatus ResponseStatus { get; set; }
    }
}