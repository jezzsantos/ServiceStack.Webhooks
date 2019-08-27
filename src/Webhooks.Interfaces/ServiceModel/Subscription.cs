using System;

namespace ServiceStack.Webhooks.ServiceModel
{
    public static class Subscription
    {
        public static Type[] AllSubscriptionDtos =
        {
            typeof(CreateSubscription),
            typeof(DeleteSubscription),
            typeof(GetSubscription),
            typeof(ListSubscriptions),
            typeof(SearchSubscriptions),
            typeof(UpdateSubscription),
            typeof(UpdateSubscriptionHistory)
        };
    }
}