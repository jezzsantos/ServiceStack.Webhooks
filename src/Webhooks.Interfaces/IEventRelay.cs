namespace ServiceStack.Webhooks
{
    public interface IEventRelay
    {
        void NotifySubscribers();
    }
}