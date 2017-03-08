using System.Collections.Generic;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.Relays
{
    public interface ISubscriptionService
    {
        List<SubscriptionRelayConfig> Search(string eventName);

        void UpdateResults(List<SubscriptionDeliveryResult> results);
    }
}