using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using ServiceStack.Caching;
using ServiceStack.Webhooks.ServiceModel;
using ServiceStack.Webhooks.ServiceModel.Types;
using ServiceStack.Webhooks.UnitTesting;

namespace ServiceStack.Webhooks.IntTests
{
    [TestFixture]
    public class SubscriptionServiceSpec
    {
        private static AppSelfHostBase _appHost;
        private static JsonServiceClient _client;
        private const string BaseUrl = "http://localhost:8080/";

        [OneTimeTearDown]
        public void CleanupContext()
        {
            _appHost.Dispose();
        }

        [OneTimeSetUp]
        public void InitializeContext()
        {
            _appHost = new AppHostForTesting();
            _appHost.Init();
            _appHost.Start(BaseUrl);

            _client = new JsonServiceClient(BaseUrl);
        }

        [SetUp]
        public void Initialize()
        {
            _appHost.Resolve<ICacheClient>().FlushAll();
        }

        [Test, Category("Integration")]
        public void WhenPostSubscriptionWithNullEvents_ThenThrowsBadRequest()
        {
            Assert.That(() => _client.Post(new CreateSubscription
            {
                Name = "aname",
                Events = null,
                Config = new SubscriptionConfig
                {
                    Url = "http://localhost:3333"
                }
            }), ThrowsWebServiceException.WithStatusCode(HttpStatusCode.BadRequest));
        }

        [Test, Category("Integration")]
        public void WhenPostSubscriptionWithNullConfig_ThenThrowsBadRequest()
        {
            Assert.That(() => _client.Post(new CreateSubscription
            {
                Name = "aname",
                Events = new List<string> {"aneventname"},
                Config = null
            }), ThrowsWebServiceException.WithStatusCode(HttpStatusCode.BadRequest));
        }

        [Test, Category("Integration")]
        public void WhenPostSubscription_ThenCreatesSubscription()
        {
            var subscriptions = _client.Post(new CreateSubscription
            {
                Name = "aname",
                Events = new List<string> {"anevent1", "anevent2"},
                Config = new SubscriptionConfig
                {
                    Url = "http://localhost:3333"
                }
            }).Subscriptions;

            Assert.That(2, Is.EqualTo(subscriptions.Count));
            AssertSubscriptionCreated(subscriptions[0], "anevent1", null);
            AssertSubscriptionCreated(subscriptions[1], "anevent2", null);
        }

        [Test, Category("Integration")]
        public void WhenPostSubscriptionWithSameEventNameAndUrl_ThenThrowsConflict()
        {
            _client.Post(new CreateSubscription
            {
                Name = "aname",
                Events = new List<string> {"anevent1", "anevent2"},
                Config = new SubscriptionConfig
                {
                    Url = "http://localhost:3333"
                }
            });

            Assert.That(() => _client.Post(new CreateSubscription
            {
                Name = "aname",
                Events = new List<string> {"anevent3", "anevent2"},
                Config = new SubscriptionConfig
                {
                    Url = "http://localhost:3333"
                }
            }), ThrowsWebServiceException.WithStatusCode(HttpStatusCode.Conflict));
        }

        private void AssertSubscriptionCreated(WebhookSubscription subscription, string eventName, string userId)
        {
            Assert.That(eventName, Is.EqualTo(subscription.Event));
            Assert.That(subscription.CreatedById, Is.EqualTo(userId));
            Assert.That(DateTime.UtcNow, Is.EqualTo(subscription.CreatedDateUtc).Within(5).Seconds);
            Assert.That(DateTime.UtcNow, Is.EqualTo(subscription.LastModifiedDateUtc).Within(5).Seconds);
            Assert.That(subscription.Id.HasValue());
        }
    }
}