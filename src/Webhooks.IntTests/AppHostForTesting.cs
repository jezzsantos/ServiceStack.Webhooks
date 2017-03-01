using Funq;
using ServiceStack.Logging;
using ServiceStack.Validation;

namespace ServiceStack.Webhooks.IntTests
{
    public class AppHostForTesting : AppSelfHostBase
    {
        public AppHostForTesting() : base("AppHostForTesting", typeof(AppHostForTesting).Assembly)
        {
        }

        public override void Configure(Container container)
        {
            Licensing.LicenseServiceStackRuntime();
            LogManager.LogFactory = new ConsoleLogFactory();
            Plugins.Add(new ValidationFeature());
            Plugins.Add(new WebhookFeature());
        }
    }
}