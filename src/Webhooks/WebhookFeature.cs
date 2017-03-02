using Funq;
using ServiceStack.Validation;
using ServiceStack.Web;
using ServiceStack.Webhooks.Clients;
using ServiceStack.Webhooks.Relays;
using ServiceStack.Webhooks.Relays.Clients;
using ServiceStack.Webhooks.ServiceInterface;
using ServiceStack.Webhooks.ServiceModel;

namespace ServiceStack.Webhooks
{
    public class WebhookFeature : IPlugin
    {
        public const string DefaultAccessRoles = @"user";
        public const string DefaultSearchRoles = @"service";
        public static readonly string[] RoleDelimiters = {",", ";"};

        public WebhookFeature()
        {
            IncludeSubscriptionService = true;
            SubscriptionAccessRoles = DefaultAccessRoles;
            SubscriptionSearchRoles = DefaultSearchRoles;
        }

        public bool IncludeSubscriptionService { get; set; }

        public string SubscriptionAccessRoles { get; set; }

        public string SubscriptionSearchRoles { get; set; }

        public void Register(IAppHost appHost)
        {
            var container = appHost.GetContainer();

            RegisterSubscriptionStore(container);
            RegisterSubscriptionService(appHost);
            RegisterClient(container);
        }

        private static void RegisterClient(Container container)
        {
            if (!container.Exists<IWebhookEventSink>())
            {
                container.RegisterAutoWiredAs<SubscriptionService, ISubscriptionService>();
                container.RegisterAutoWiredAs<CacheClientEventSubscriptionCache, IWebhookEventSubscriptionCache>();
                container.RegisterAutoWiredAs<EventServiceClientFactory, IEventServiceClientFactory>();
                container.RegisterAutoWiredAs<EventServiceClient, IWebhookEventServiceClient>();
                container.RegisterAutoWiredAs<AppHostWebhookEventSink, IWebhookEventSink>();
            }
            container.RegisterAutoWiredAs<WebhooksClient, IWebhooks>();
        }

        private static void RegisterSubscriptionStore(Container container)
        {
            if (!container.Exists<IWebhookSubscriptionStore>())
            {
                container.RegisterAutoWiredAs<MemoryWebhookSubscriptionStore, IWebhookSubscriptionStore>();
            }
        }

        private void RegisterSubscriptionService(IAppHost appHost)
        {
            if (IncludeSubscriptionService)
            {
                var container = appHost.GetContainer();
                appHost.RegisterService(typeof(SubscriptionService));

                container.RegisterValidators(typeof(WebHookInterfaces).Assembly, typeof(SubscriptionService).Assembly);
                container.RegisterAutoWiredAs<SubscriptionEventsValidator, ISubscriptionEventsValidator>();
                container.RegisterAutoWiredAs<SubscriptionConfigValidator, ISubscriptionConfigValidator>();

                container.RegisterAutoWiredAs<AuthSessionCurrentCaller, ICurrentCaller>();

                if (appHost.Plugins.Exists(plugin => plugin is AuthFeature))
                {
                    appHost.GlobalRequestFilters.Add(AuthenticationFilter);
                }
            }
        }

        internal void AuthenticationFilter(IRequest request, IResponse response, object dto)
        {
            if (IsSubscriptionService(request.PathInfo))
            {
                new AuthenticateAttribute().Execute(request, response, dto);

                var requiredRoles = (request.PathInfo.EqualsIgnoreCase(new SearchSubscriptions().ToGetUrl())
                        ? SubscriptionSearchRoles
                        : SubscriptionAccessRoles)
                    .SafeSplit(RoleDelimiters);
                RequiresAnyRoleAttribute.AssertRequiredRoles(request, requiredRoles);
            }
        }

        private static bool IsSubscriptionService(string pathInfo)
        {
            return pathInfo.StartsWith(Subscription.RootPath);
        }
    }
}