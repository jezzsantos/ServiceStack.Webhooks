using System.Collections.Generic;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.ServiceInterface
{
    public interface ISubscriptionService
    {
        List<SubscriptionConfig> Search(string eventName);
    }
}