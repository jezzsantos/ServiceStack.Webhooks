using System;
using ServiceStack.Logging;

namespace ServiceStack.Webhooks.Relays.Clients
{
    public class EventServiceClientFactory : IEventServiceClientFactory
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(EventServiceClientFactory));

        public IServiceClient Create(string url)
        {
            try
            {
                return new ServiceClient(url);
            }
            catch (Exception ex)
            {
                logger.Error(@"[ServiceStack.Webhooks.Relays.Clients.EventServiceClientFactory] Failed to create a serviceclient to subscriber at {0}".Fmt(url), ex);
                return null;
            }
        }
    }
}