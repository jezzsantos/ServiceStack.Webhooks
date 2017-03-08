namespace ServiceStack.Webhooks.ServiceModel.Types
{
    public class SubscriptionRelayConfig
    {
        public string SubscriptionId { get; set; }

        public SubscriptionConfig Config { get; set; }
    }
}