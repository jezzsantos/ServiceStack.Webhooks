namespace ServiceStack.Webhooks.Relays.Clients
{
    public interface IEventServiceClientFactory
    {
        IServiceClient Create(string url);
    }
}