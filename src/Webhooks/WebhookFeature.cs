using System;
using System.Linq;
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
        public const string DefaultSubscriberRoles = @"user";
        public const string DefaultRelayRoles = @"service";
        public static readonly string[] RoleDelimiters = {",", ";"};

        public WebhookFeature()
        {
            IncludeSubscriptionService = true;
            SecureSubscriberRoles = DefaultSubscriberRoles;
            SecureRelayRoles = DefaultRelayRoles;
        }

        public bool IncludeSubscriptionService { get; set; }

        public string SecureSubscriberRoles { get; set; }

        public string SecureRelayRoles { get; set; }

        public Action<WebhookEvent> PublishEventFilter { get; set; }

        public void Register(IAppHost appHost)
        {
            var container = appHost.GetContainer();

            RegisterSubscriptionStore(container);
            RegisterSubscriptionService(appHost);
            RegisterClient(container);
        }

        private void RegisterClient(Container container)
        {
            if (!container.Exists<IEventSink>())
            {
                container.RegisterAutoWiredAs<SubscriptionService, ISubscriptionService>();
                container.RegisterAutoWiredAs<CacheClientEventSubscriptionCache, IEventSubscriptionCache>();
                container.RegisterAutoWiredAs<EventServiceClientFactory, IEventServiceClientFactory>();
                container.RegisterAutoWiredAs<EventServiceClient, IEventServiceClient>();
                container.RegisterAutoWiredAs<AppHostEventSink, IEventSink>();
            }

            container.Register<IWebhooks>(x => new WebhooksClient
            {
                EventSink = x.Resolve<IEventSink>(),
                PublishFilter = PublishEventFilter
            }).ReusedWithin(ReuseScope.None);
        }

        private static void RegisterSubscriptionStore(Container container)
        {
            if (!container.Exists<ISubscriptionStore>())
            {
                container.RegisterAutoWiredAs<MemorySubscriptionStore, ISubscriptionStore>();
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
                container.RegisterAutoWiredAs<SubscriptionDeliveryResultValidator, ISubscriptionDeliveryResultValidator>();

                container.RegisterAutoWiredAs<AuthSessionCurrentCaller, ICurrentCaller>().ReusedWithin(ReuseScope.Request);

                if (appHost.Plugins.Exists(plugin => plugin is AuthFeature))
                {
                    appHost.GlobalRequestFilters.Add(AuthorizeSubscriptionServiceRequests);
                }
            }
        }

        public void AuthorizeSubscriptionServiceRequests(IRequest request, IResponse response, object dto)
        {
            if (IsSubscriptionService(request.Dto.GetType()))
            {
                var attribute = new AuthenticateAttribute();
                AsyncHelper.RunSync(() => attribute.ExecuteAsync(request, response, dto));

                var requiredRoles = GetRequiredRoles(request.Dto);
                if (requiredRoles.Length > 0)
                {
                    RequiresAnyRoleAttribute.AssertRequiredRoles(request, requiredRoles);
                }
            }
        }

        private static bool IsSubscriptionService(Type dtoType)
        {
            return Subscription.AllSubscriptionDtos.Contains(dtoType);
        }

        private string[] GetRequiredRoles(object dto)
        {
            if (dto is SearchSubscriptions || dto is UpdateSubscriptionHistory)
            {
                return SecureRelayRoles.SafeSplit(RoleDelimiters);
            }

            return SecureSubscriberRoles.SafeSplit(RoleDelimiters);
        }
    }
}