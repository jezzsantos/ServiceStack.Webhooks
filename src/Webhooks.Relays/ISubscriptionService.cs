using System.Collections.Generic;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.Relays
{
    public interface ISubscriptionService
    {
        List<SubscriptionConfig> Search(string eventName);
    }
}
