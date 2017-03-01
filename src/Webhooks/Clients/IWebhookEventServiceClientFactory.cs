namespace ServiceStack.Webhooks.Clients
{
    public interface IWebhookEventServiceClientFactory
    {
        IServiceClient Create(string url);
    }
}