using System;
using System.Collections.Generic;
using System.Net;
using Funq;
using NUnit.Framework;
using ServiceStack.Testing;
using ServiceStack.Webhooks.Clients;
using ServiceStack.Webhooks.Relays;
using ServiceStack.Webhooks.Relays.Clients;
using ServiceStack.Webhooks.ServiceInterface;
using ServiceStack.Webhooks.ServiceModel;
using ServiceStack.Webhooks.ServiceModel.Types;
using ServiceStack.Webhooks.UnitTesting;

namespace ServiceStack.Webhooks.UnitTests
{
    public class WebhookFeatureSpec
    {
        [TestFixture]
        public class GivenNoContext
        {
            private static ServiceStackHost appHost;
            private Container container;

            [TearDown]
            public void CleanupAllContexts()
            {
                appHost.Dispose();
            }

            [SetUp]
            public void InitializeContext()
            {
                appHost = new BasicAppHost();
                appHost.Init();

                container = appHost.Container;
            }

            [Test, Category("Unit")]
            public void WhenCtor_ThenDefaultProperties()
            {
                var result = new WebhookFeature();

                Assert.That(result.IncludeSubscriptionService, Is.True);
                Assert.That(result.SubscriptionAccessRoles, Is.EqualTo(WebhookFeature.DefaultAccessRoles));
                Assert.That(result.SubscriptionSearchRoles, Is.EqualTo(WebhookFeature.DefaultSearchRoles));
            }

            [Test, Category("Unit")]
            public void WhenRegisterAndIncludeSubscriptionService_ThenSubscriptionServiceDependenciesRegistered()
            {
                new WebhookFeature().Register(appHost);

                Assert.That(container.GetService(typeof(ICurrentCaller)), Is.TypeOf<AuthSessionCurrentCaller>());
            }

            [Test, Category("Unit")]
            public void WhenRegisterAndIncludeSubscriptionServiceAndAuthFeature_ThenAuthorizeSubscriptionServiceRequestsAdded()
            {
                appHost.Plugins.Add(new AuthFeature(() => null, null));

                var feature = new WebhookFeature();
                feature.Register(appHost);

                Assert.That(appHost.GlobalRequestFilters.Exists(action => action == feature.AuthorizeSubscriptionServiceRequests), Is.True);
            }

            [Test, Category("Unit")]
            public void WhenRegisterAndIncludeSubscriptionServiceAndAuthFeature_ThenAuthorizeSubscriptionServiceRequestsNotAdded()
            {
                var feature = new WebhookFeature();
                feature.Register(appHost);

                Assert.That(appHost.GlobalRequestFilters.Exists(action => action == feature.AuthorizeSubscriptionServiceRequests), Is.False);
            }

            [Test, Category("Unit")]
            public void WhenRegisterAndSubscriptionStoreNotRegistered_ThenRegistersMemoryStoreByDefault()
            {
                new WebhookFeature().Register(appHost);

                Assert.That(container.GetService(typeof(IWebhookSubscriptionStore)), Is.TypeOf<MemoryWebhookSubscriptionStore>());
            }

            [Test, Category("Unit")]
            public void WhenRegisterAndSubscriptionStoreAlreadyRegistered_ThenDoesNotRegisterMemoryStoreByDefault()
            {
                container.Register<IWebhookSubscriptionStore>(new TestSubscriptionStore());

                new WebhookFeature().Register(appHost);

                Assert.That(container.GetService(typeof(IWebhookSubscriptionStore)), Is.TypeOf<TestSubscriptionStore>());
            }

            [Test, Category("Unit")]
            public void WhenRegister_ThenClientDependenciesRegistered()
            {
                new WebhookFeature().Register(appHost);

                Assert.That(container.GetService(typeof(IWebhooks)), Is.TypeOf<WebhooksClient>());
                Assert.That(container.GetService(typeof(IWebhookEventSubscriptionCache)), Is.TypeOf<CacheClientEventSubscriptionCache>());
                Assert.That(container.GetService(typeof(IEventServiceClientFactory)), Is.TypeOf<EventServiceClientFactory>());
                Assert.That(container.GetService(typeof(IWebhookEventServiceClient)), Is.TypeOf<EventServiceClient>());
            }

            [Test, Category("Unit")]
            public void WhenRegisterAndEventSinkNotRegistered_ThenRegistersAppHostSinkByDefault()
            {
                new WebhookFeature().Register(appHost);

                Assert.That(container.GetService(typeof(IWebhookEventSink)), Is.TypeOf<AppHostWebhookEventSink>());
                Assert.That(container.GetService(typeof(ISubscriptionService)), Is.TypeOf<SubscriptionService>());
            }

            [Test, Category("Unit")]
            public void WhenRegisterAndEventSinkAlreadyRegistered_ThenDoesNotRegisterAppHostSinkByDefault()
            {
                container.Register<IWebhookEventSink>(new TestEventSink());

                new WebhookFeature().Register(appHost);

                Assert.That(container.GetService(typeof(IWebhookEventSink)), Is.TypeOf<TestEventSink>());
                Assert.That(container.GetService(typeof(ISubscriptionService)), Is.Null);
            }

            [Test, Category("Unit")]
            public void WhenAuthorizeSubscriptionServiceRequestsAndNotForSubscriptionService_ThenDoesNotAuthenticate()
            {
                var feature = new WebhookFeature();
                feature.Register(appHost);
                var request = new MockHttpRequest();
                request.PathInfo = "/aresource";

                feature.AuthorizeSubscriptionServiceRequests(request, null, new TestDto());
            }

            [Test, Category("Unit")]
            public void WhenAuthorizeSubscriptionServiceRequestsAndForSubscriptionServiceAndUnauthenticated_ThenThrowsUnauthorized()
            {
                var feature = new WebhookFeature();
                feature.Register(appHost);
                var request = new MockHttpRequest
                {
                    PathInfo = new GetSubscription
                    {
                        Id = "asubscriptionid"
                    }.ToGetUrl()
                };
                var response = new MockHttpResponse(request);

                Assert.That(() => feature.AuthorizeSubscriptionServiceRequests(request, response, new TestDto()), ThrowsHttpError.WithStatusCode(HttpStatusCode.Unauthorized));
            }

            [Test, Category("Unit")]
            public void WhenAuthorizeSubscriptionServiceRequestsAndForSubscriptionServiceAndAuthenticatedInWrongRole_ThenThrowsForbidden()
            {
                var feature = new WebhookFeature();
                feature.Register(appHost);
                var request = new MockHttpRequest
                {
                    PathInfo = new GetSubscription
                    {
                        Id = "asubscriptionid"
                    }.ToGetUrl()
                };
                request.Items.Add(Keywords.Session, new AuthUserSession
                {
                    IsAuthenticated = true
                });
                var response = new MockHttpResponse(request);

                Assert.That(() => feature.AuthorizeSubscriptionServiceRequests(request, response, new TestDto()), ThrowsHttpError.WithStatusCode(HttpStatusCode.Forbidden));
            }

            [Test, Category("Unit")]
            public void WhenAuthorizeSubscriptionServiceRequestsAndForSubscriptionServiceAndAuthenticatedAndNoAccessRoles_ThenAuthorized()
            {
                var feature = new WebhookFeature {SubscriptionAccessRoles = null};
                feature.Register(appHost);
                var request = new MockHttpRequest
                {
                    PathInfo = new GetSubscription
                    {
                        Id = "asubscriptionid"
                    }.ToGetUrl()
                };
                request.Items.Add(Keywords.Session, new AuthUserSession
                {
                    IsAuthenticated = true
                });
                var response = new MockHttpResponse(request);

                feature.AuthorizeSubscriptionServiceRequests(request, response, new TestDto());
            }

            [Test, Category("Unit")]
            public void WhenAuthorizeSubscriptionServiceRequestsAndForSubscriptionServiceAndAuthenticatedAndNoSearchRoles_ThenAuthorized()
            {
                var feature = new WebhookFeature {SubscriptionAccessRoles = null};
                feature.Register(appHost);
                var request = new MockHttpRequest
                {
                    PathInfo = new GetSubscription
                    {
                        Id = "asubscriptionid"
                    }.ToGetUrl()
                };
                request.Items.Add(Keywords.Session, new AuthUserSession
                {
                    IsAuthenticated = true
                });
                var response = new MockHttpResponse(request);

                feature.AuthorizeSubscriptionServiceRequests(request, response, new TestDto());
            }

            [Test, Category("Unit")]
            public void WhenAuthorizeSubscriptionServiceRequestsAndForSubscriptionServiceAndAuthenticatedInAccessRole_ThenAuthorized()
            {
                var feature = new WebhookFeature();
                feature.Register(appHost);
                var request = new MockHttpRequest
                {
                    PathInfo = new GetSubscription
                    {
                        Id = "asubscriptionid"
                    }.ToGetUrl()
                };
                request.Items.Add(Keywords.Session, new AuthUserSession
                {
                    IsAuthenticated = true,
                    UserAuthId = "auserid",
                    Roles = new List<string>(feature.SubscriptionAccessRoles.SafeSplit(","))
                });
                var response = new MockHttpResponse(request);

                feature.AuthorizeSubscriptionServiceRequests(request, response, new TestDto());
            }

            [Test, Category("Unit")]
            public void WhenAuthorizeSubscriptionServiceRequestsAndForSubscriptionServiceAndAuthenticatedInSearchRole_ThenAuthorized()
            {
                var feature = new WebhookFeature();
                feature.Register(appHost);
                var request = new MockHttpRequest
                {
                    PathInfo = new SearchSubscriptions().ToGetUrl()
                };
                request.Items.Add(Keywords.Session, new AuthUserSession
                {
                    IsAuthenticated = true,
                    UserAuthId = "auserid",
                    Roles = new List<string>(feature.SubscriptionSearchRoles.SafeSplit(","))
                });
                var response = new MockHttpResponse(request);

                feature.AuthorizeSubscriptionServiceRequests(request, response, new TestDto());
            }
        }

        public class TestDto
        {
        }

        public class TestSubscriptionStore : IWebhookSubscriptionStore
        {
            public string Add(WebhookSubscription subscription)
            {
                throw new NotImplementedException();
            }

            public List<WebhookSubscription> Find(string userId)
            {
                throw new NotImplementedException();
            }

            public WebhookSubscription Get(string userId, string eventName)
            {
                throw new NotImplementedException();
            }

            public void Update(string subscriptionId, WebhookSubscription subscription)
            {
                throw new NotImplementedException();
            }

            public void Delete(string subscriptionId)
            {
                throw new NotImplementedException();
            }

            public List<SubscriptionConfig> Search(string eventName)
            {
                throw new NotImplementedException();
            }
        }

        public class TestEventSink : IWebhookEventSink
        {
            public void Write<TDto>(string eventName, TDto data)
            {
                throw new NotImplementedException();
            }
        }
    }
}