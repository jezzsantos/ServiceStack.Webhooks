using Microsoft.Azure;

namespace ServiceStack.Webhooks.Azure.Settings
{
    internal interface ICloudConfigurationProvider
    {
        /// <summary>
        ///     Gets the duration of the cache
        /// </summary>
        int CacheDuration { get; }

        /// <summary>
        ///     Gets the setting with the specified name
        /// </summary>
        string GetSetting(string settingName);
    }

    internal class CloudConfigurationProvider : ICloudConfigurationProvider
    {
        internal const string CloudConfigurationCacheDurationSettingName =
            @"CloudConfigurationSettings.CacheDuration";
        private const int DefaultCacheDuration = 60;

        public string GetSetting(string settingName)
        {
            return CloudConfigurationManager.GetSetting(settingName);
        }

        public int CacheDuration
        {
            get
            {
                var durationValue = CloudConfigurationManager.GetSetting(CloudConfigurationCacheDurationSettingName);
                if (durationValue.HasValue())
                {
                    int result;
                    if (int.TryParse(durationValue, out result))
                    {
                        return result;
                    }
                }

                return DefaultCacheDuration;
            }
        }
    }
}