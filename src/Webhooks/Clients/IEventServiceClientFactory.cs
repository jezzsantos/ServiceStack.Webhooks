namespace ServiceStack.Webhooks.Clients
{
    public interface IEventServiceClientFactory
    {
        IServiceClient Create(string url);
    }
}