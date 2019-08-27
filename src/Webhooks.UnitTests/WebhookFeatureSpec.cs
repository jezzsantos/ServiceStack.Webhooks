using System;
using System.Collections.Generic;
using System.Net;
using Funq;
using NUnit.Framework;
using ServiceStack.Auth;
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
                Assert.That(result.SecureSubscriberRoles, Is.EqualTo(WebhookFeature.DefaultSubscriberRoles));
                Assert.That(result.SecureRelayRoles, Is.EqualTo(WebhookFeature.DefaultRelayRoles));
            }

            [Test, Category("Unit")]
            public void WhenRegisterAndPublishEventFilter_ThenWebhooksClientRegisteredWithPublishFilter()
            {
                var filter = new Action<WebhookEvent>(e => { });
                new WebhookFeature
                {
                    PublishEventFilter = filter
                }.Register(appHost);

                Assert.That(((WebhooksClient) container.GetService(typeof(IWebhooks))).PublishFilter, Is.EqualTo(filter));
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
                appHost.Plugins.Add(new AuthFeature(null, new IAuthProvider[0]));

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

                Assert.That(container.GetService(typeof(ISubscriptionStore)), Is.TypeOf<MemorySubscriptionStore>());
            }

            [Test, Category("Unit")]
            public void WhenRegisterAndSubscriptionStoreAlreadyRegistered_ThenDoesNotRegisterMemoryStoreByDefault()
            {
                container.Register<ISubscriptionStore>(new TestSubscriptionStore());

                new WebhookFeature().Register(appHost);

                Assert.That(container.GetService(typeof(ISubscriptionStore)), Is.TypeOf<TestSubscriptionStore>());
            }

            [Test, Category("Unit")]
            public void WhenRegister_ThenClientDependenciesRegistered()
            {
                new WebhookFeature().Register(appHost);

                Assert.That(container.GetService(typeof(IWebhooks)), Is.TypeOf<WebhooksClient>());
                Assert.That(container.GetService(typeof(IEventSubscriptionCache)), Is.TypeOf<CacheClientEventSubscriptionCache>());
                Assert.That(container.GetService(typeof(IEventServiceClientFactory)), Is.TypeOf<EventServiceClientFactory>());
                Assert.That(container.GetService(typeof(IEventServiceClient)), Is.TypeOf<EventServiceClient>());
            }

            [Test, Category("Unit")]
            public void WhenRegisterAndEventSinkNotRegistered_ThenRegistersAppHostSinkByDefault()
            {
                new WebhookFeature().Register(appHost);

                Assert.That(container.GetService(typeof(IEventSink)), Is.TypeOf<AppHostEventSink>());
                Assert.That(container.GetService(typeof(ISubscriptionService)), Is.TypeOf<SubscriptionService>());
            }

            [Test, Category("Unit")]
            public void WhenRegisterAndEventSinkAlreadyRegistered_ThenDoesNotRegisterAppHostSinkByDefault()
            {
                container.Register<IEventSink>(new TestEventSink());

                new WebhookFeature().Register(appHost);

                Assert.That(container.GetService(typeof(IEventSink)), Is.TypeOf<TestEventSink>());
                Assert.That(container.GetService(typeof(ISubscriptionService)), Is.Null);
            }

            [Test, Category("Unit")]
            public void WhenAuthorizeSubscriptionServiceRequestsAndNotForSubscriptionService_ThenDoesNotAuthenticate()
            {
                var feature = new WebhookFeature();
                feature.Register(appHost);
                var request = new MockHttpRequest
                {
                    Dto = "anotherresource"
                };

                feature.AuthorizeSubscriptionServiceRequests(request, null, new TestDto());
            }

            [Test, Category("Unit")]
            public void WhenAuthorizeSubscriptionServiceRequestsAndForSubscriptionServiceAndUnauthenticated_ThenThrowsUnauthorized()
            {
                var feature = new WebhookFeature();
                feature.Register(appHost);
                var request = new MockHttpRequest
                {
                    Dto = new GetSubscription()
                };
                var response = new MockHttpResponse(request);

                Assert.That(() => feature.AuthorizeSubscriptionServiceRequests(request, response, new TestDto()), ThrowsHttpError.WithStatusCode(HttpStatusCode.Unauthorized));
            }

            [Test, Category("Unit")]
            public void WhenAuthorizeSubscriptionServiceRequestsAndForGetSubscriptionAndAuthenticatedInWrongRole_ThenThrowsForbidden()
            {
                var feature = new WebhookFeature();
                feature.Register(appHost);
                var request = new MockHttpRequest
                {
                    PathInfo = new GetSubscription
                    {
                        Id = "asubscriptionid"
                    }.ToGetUrl(),
                    Dto = new GetSubscription()
                };
                request.Items.Add(Keywords.Session, new AuthUserSession
                {
                    IsAuthenticated = true,
                    Roles = new List<string> {"anotherrole"}
                });
                var response = new MockHttpResponse(request);

                Assert.That(() => feature.AuthorizeSubscriptionServiceRequests(request, response, new TestDto()), ThrowsHttpError.WithStatusCode(HttpStatusCode.Forbidden));
            }

            [Test, Category("Unit")]
            public void WhenAuthorizeSubscriptionServiceRequestsAndForGetSubscriptionAndAuthenticatedAndNoSubscriberRoles_ThenAuthorized()
            {
                var feature = new WebhookFeature {SecureSubscriberRoles = null};
                feature.Register(appHost);
                var request = new MockHttpRequest
                {
                    PathInfo = new GetSubscription
                    {
                        Id = "asubscriptionid"
                    }.ToGetUrl(),
                    Dto = new GetSubscription()
                };
                request.Items.Add(Keywords.Session, new AuthUserSession
                {
                    IsAuthenticated = true
                });
                var response = new MockHttpResponse(request);

                feature.AuthorizeSubscriptionServiceRequests(request, response, new TestDto());
            }

            [Test, Category("Unit")]
            public void WhenAuthorizeSubscriptionServiceRequestsAndForGetSubscriptionAndAuthenticatedInSubscriberRole_ThenAuthorized()
            {
                var feature = new WebhookFeature();
                feature.Register(appHost);
                var request = new MockHttpRequest
                {
                    PathInfo = new GetSubscription
                    {
                        Id = "asubscriptionid"
                    }.ToGetUrl(),
                    Dto = new GetSubscription()
                };
                request.Items.Add(Keywords.Session, new AuthUserSession
                {
                    IsAuthenticated = true,
                    UserAuthId = "auserid",
                    Roles = new List<string>(feature.SecureSubscriberRoles.SafeSplit(","))
                });
                var response = new MockHttpResponse(request);

                feature.AuthorizeSubscriptionServiceRequests(request, response, new TestDto());
            }

            [Test, Category("Unit")]
            public void WhenAuthorizeSubscriptionServiceRequestsAndForSearchSubscriptionAndAuthenticatedInWrongRole_ThenThrowsForbidden()
            {
                var feature = new WebhookFeature();
                feature.Register(appHost);
                var request = new MockHttpRequest
                {
                    PathInfo = new SearchSubscriptions().ToGetUrl(),
                    Dto = new SearchSubscriptions()
                };
                request.Items.Add(Keywords.Session, new AuthUserSession
                {
                    IsAuthenticated = true,
                    UserAuthId = "auserid",
                    Roles = new List<string> {"anotherrole"}
                });
                var response = new MockHttpResponse(request);

                Assert.That(() => feature.AuthorizeSubscriptionServiceRequests(request, response, new TestDto()), ThrowsHttpError.WithStatusCode(HttpStatusCode.Forbidden));
            }

            [Test, Category("Unit")]
            public void WhenAuthorizeSubscriptionServiceRequestsAndForSearchSubscriptionAndAuthenticatedAndNoRelayRoles_ThenAuthorized()
            {
                var feature = new WebhookFeature {SecureRelayRoles = null};
                feature.Register(appHost);
                var request = new MockHttpRequest
                {
                    PathInfo = new SearchSubscriptions().ToGetUrl(),
                    Dto = new SearchSubscriptions()
                };
                request.Items.Add(Keywords.Session, new AuthUserSession
                {
                    IsAuthenticated = true
                });
                var response = new MockHttpResponse(request);

                feature.AuthorizeSubscriptionServiceRequests(request, response, new TestDto());
            }

            [Test, Category("Unit")]
            public void WhenAuthorizeSubscriptionServiceRequestsAndForUpdateHistorySubscriptionAndAuthenticatedInWrongRole_ThenThrowsForbidden()
            {
                var feature = new WebhookFeature();
                feature.Register(appHost);
                var request = new MockHttpRequest
                {
                    PathInfo = new UpdateSubscriptionHistory().ToPutUrl(),
                    Dto = new UpdateSubscriptionHistory()
                };
                request.Items.Add(Keywords.Session, new AuthUserSession
                {
                    IsAuthenticated = true,
                    UserAuthId = "auserid",
                    Roles = new List<string> {"anotherrole"}
                });
                var response = new MockHttpResponse(request);

                Assert.That(() => feature.AuthorizeSubscriptionServiceRequests(request, response, new TestDto()), ThrowsHttpError.WithStatusCode(HttpStatusCode.Forbidden));
            }

            [Test, Category("Unit")]
            public void WhenAuthorizeSubscriptionServiceRequestsAndForUpdateHistorySubscriptionAndAuthenticatedAndNoRelayRoles_ThenAuthorized()
            {
                var feature = new WebhookFeature {SecureRelayRoles = null};
                feature.Register(appHost);
                var request = new MockHttpRequest
                {
                    PathInfo = new UpdateSubscriptionHistory().ToPutUrl(),
                    Dto = new UpdateSubscriptionHistory()
                };
                request.Items.Add(Keywords.Session, new AuthUserSession
                {
                    IsAuthenticated = true
                });
                var response = new MockHttpResponse(request);

                feature.AuthorizeSubscriptionServiceRequests(request, response, new TestDto());
            }

            [Test, Category("Unit")]
            public void WhenAuthorizeSubscriptionServiceRequestsAndForSearchSubscriptionAndAuthenticatedInRelayRole_ThenAuthorized()
            {
                var feature = new WebhookFeature();
                feature.Register(appHost);
                var request = new MockHttpRequest
                {
                    PathInfo = new SearchSubscriptions().ToGetUrl(),
                    Dto = new SearchSubscriptions()
                };
                request.Items.Add(Keywords.Session, new AuthUserSession
                {
                    IsAuthenticated = true,
                    UserAuthId = "auserid",
                    Roles = new List<string>(feature.SecureRelayRoles.SafeSplit(","))
                });
                var response = new MockHttpResponse(request);

                feature.AuthorizeSubscriptionServiceRequests(request, response, new TestDto());
            }
        }

        public class TestDto
        {
        }

        public class TestSubscriptionStore : ISubscriptionStore
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

            public WebhookSubscription Get(string subscriptionId)
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

            public List<SubscriptionRelayConfig> Search(string eventName, bool? isActive)
            {
                throw new NotImplementedException();
            }

            public void Add(string subscriptionId, SubscriptionDeliveryResult result)
            {
                throw new NotImplementedException();
            }

            public List<SubscriptionDeliveryResult> Search(string subscriptionId, int top)
            {
                throw new NotImplementedException();
            }
        }

        public class TestEventSink : IEventSink
        {
            public void Write(WebhookEvent webhookEvent)
            {
                throw new NotImplementedException();
            }
        }
    }
}