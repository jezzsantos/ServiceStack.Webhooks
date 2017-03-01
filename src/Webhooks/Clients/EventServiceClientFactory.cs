using System;
using ServiceStack.Logging;

namespace ServiceStack.Webhooks.Clients
{
    internal class EventServiceClientFactory : IEventServiceClientFactory
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
                logger.Error(@"Failed to create a serviceclient to subscriber at {0}".Fmt(url), ex);
                return null;
            }
        }
    }
}