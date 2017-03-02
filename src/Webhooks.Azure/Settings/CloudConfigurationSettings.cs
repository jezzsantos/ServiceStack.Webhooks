using System;
using System.Collections.Generic;

namespace ServiceStack.Webhooks.Azure.Settings
{
    /// <summary>
    ///     Defines configurations settings stored in an cloud role configuration file.
    /// </summary>
    public class CloudConfigurationSettings
    {
        private readonly ICloudConfigurationProvider configurationProvider;
        private DateTime cacheRefreshedNext = DateTime.MinValue;

        public CloudConfigurationSettings()
            : this(new CloudConfigurationProvider())
        {
        }

        internal CloudConfigurationSettings(ICloudConfigurationProvider provider)
        {
            Guard.AgainstNull(() => provider, provider);

            configurationProvider = provider;
            CachedSettings = new Dictionary<string, string>();
            ClearCache();
        }

        internal IDictionary<string, string> CachedSettings { get; private set; }

        /// <summary>
        ///     Returns the setting with the specified name
        /// </summary>
        /// <param name="settingName"> The name of the setting </param>
        public virtual string GetSetting(string settingName)
        {
            RefreshCacheIfOutOfDate();

            lock (CachedSettings)
            {
                if (!CachedSettings.ContainsKey(settingName))
                {
                    var settingValue = configurationProvider.GetSetting(settingName);
                    CachedSettings.Add(settingName, settingValue);

                    return settingValue;
                }
            }

            return CachedSettings[settingName];
        }

        internal void ClearCache()
        {
            CachedSettings.Clear();
            cacheRefreshedNext = DateTime.UtcNow + TimeSpan.FromSeconds(configurationProvider.CacheDuration);
        }

        private void RefreshCacheIfOutOfDate()
        {
            var now = DateTime.UtcNow;
            if (now >= cacheRefreshedNext)
            {
                ClearCache();
            }
        }
    }
}