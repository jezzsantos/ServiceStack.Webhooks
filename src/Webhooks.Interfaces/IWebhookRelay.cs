namespace ServiceStack.Webhooks
{
    public interface IWebhookRelay
    {
        void NotifySubscribers();
    }
}