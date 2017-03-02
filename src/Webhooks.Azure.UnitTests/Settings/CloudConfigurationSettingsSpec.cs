using System.Threading;
using Moq;
using NUnit.Framework;
using ServiceStack.Webhooks.Azure.Settings;

namespace ServiceStack.Webhooks.Azure.UnitTests.Settings
{
    public class CloudConfigurationSettingsSpec
    {
        [TestFixture]
        public class GivenAContext
        {
            private CloudConfigurationSettings configuration;
            private Mock<ICloudConfigurationProvider> configurationProvider;

            [SetUp]
            public void Initialize()
            {
                configurationProvider = new Mock<ICloudConfigurationProvider>();
                configurationProvider.Setup(cp => cp.GetSetting(It.IsAny<string>()))
                    .Returns("avalue");
                configurationProvider.Setup(cp => cp.CacheDuration)
                    .Returns(60);
                configuration = new CloudConfigurationSettings(configurationProvider.Object);
            }

            [TearDown]
            public void Cleanup()
            {
                configuration.ClearCache();
            }

            [Test, Category("Unit")]
            public void WhenGetSetting_ThenCachesAndReturnsSettingValue()
            {
                var result = configuration.GetSetting("asettingname");

                Assert.That(result, Is.EqualTo("avalue"));
                Assert.That(configuration.CachedSettings.ContainsKey("asettingname"));
                configurationProvider.Verify(cfgp => cfgp.GetSetting(It.IsAny<string>()), Times.Once());
                configurationProvider.VerifyGet(cfgp => cfgp.CacheDuration, Times.Once());
            }

            [Test, Category("Unit")]
            public void WhenGetSettingWithCachedValue_ThenReturnsCachedSettingValue()
            {
                var result = configuration.GetSetting("asettingname");

                Assert.That(result, Is.EqualTo("avalue"));

                var result2 = configuration.GetSetting("asettingname");

                Assert.That(result2, Is.EqualTo("avalue"));
                Assert.That(configuration.CachedSettings.ContainsKey("asettingname"));
                configurationProvider.Verify(cfgp => cfgp.GetSetting(It.IsAny<string>()), Times.Once());
                configurationProvider.VerifyGet(cfgp => cfgp.CacheDuration, Times.Once());
            }

            [Test, Category("Unit")]
            public void WhenClearCacheWithCachedValue_ThenCachesAndReturnsSettingValue()
            {
                var result = configuration.GetSetting("asettingname");
                Assert.That(result, Is.EqualTo("avalue"));

                configuration.ClearCache();
                Assert.False(configuration.CachedSettings.ContainsKey("asettingname"));

                var result2 = configuration.GetSetting("asettingname");

                Assert.That(result2, Is.EqualTo("avalue"));
                Assert.That(configuration.CachedSettings.ContainsKey("asettingname"));
                configurationProvider.Verify(cfgp => cfgp.GetSetting(It.IsAny<string>()), Times.Exactly(2));
                configurationProvider.VerifyGet(cfgp => cfgp.CacheDuration, Times.Exactly(2));
            }

            [Test, Category("Unit")]
            public void WhenGetSettingAndCacheNotExpired_ThenReturnsCachedSetting()
            {
                var result = configuration.GetSetting("asettingname");
                Assert.That(result, Is.EqualTo("avalue"));

                var result2 = configuration.GetSetting("asettingname");
                Assert.That(result2, Is.EqualTo("avalue"));
                Assert.That(configuration.CachedSettings.ContainsKey("asettingname"));
                configurationProvider.Verify(cfgp => cfgp.GetSetting(It.IsAny<string>()), Times.Once());
                configurationProvider.VerifyGet(cfgp => cfgp.CacheDuration, Times.Once());
            }

            [Test, Category("Unit")]
            public void WhenGetSettingAndCacheExpired_ThenReturnsSetting()
            {
                configurationProvider.Setup(cp => cp.CacheDuration)
                    .Returns(1);
                configuration.ClearCache();

                var result = configuration.GetSetting("asettingname");
                Assert.That(result, Is.EqualTo("avalue"));

                //Time out cache
                Thread.Sleep(1500);

                var result2 = configuration.GetSetting("asettingname");
                Assert.That(result2, Is.EqualTo("avalue"));
                Assert.That(configuration.CachedSettings.ContainsKey("asettingname"));
                configurationProvider.Verify(cfgp => cfgp.GetSetting(It.IsAny<string>()), Times.Exactly(2));
                configurationProvider.VerifyGet(cfgp => cfgp.CacheDuration, Times.Exactly(3));
            }
        }
    }
}