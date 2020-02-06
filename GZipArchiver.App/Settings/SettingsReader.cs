using System;
using System.Configuration;
using System.Reflection;

namespace GZipArchiver.App.Settings
{
    public static class SettingsReader<T>
    {
        public static T ReadSettings()
        {
            var settings = Activator.CreateInstance<T>();

            var appSettings = ConfigurationManager.AppSettings;

            if (!appSettings.HasKeys())
            {
                return settings;
            }

            var settingProperties = typeof(T).GetProperties();
            foreach (var settingProperty in settingProperties)
            {
                var hasSettingAttribute = Attribute.IsDefined(settingProperty, typeof(SettingAttribute));
                if (hasSettingAttribute)
                {
                    var settingAttributes = settingProperty.GetCustomAttributes(typeof(SettingAttribute), false);
                    var settingAttribute = (SettingAttribute) settingAttributes[0];
                    string settingConfigVal = appSettings.Get(settingAttribute.Key);

                    // set config value or default value
                    SetSettingPropertyValue(settings, settingProperty, settingAttribute, settingConfigVal);
                }
            }

            return settings;
        }

        private static void SetSettingPropertyValue(T settings, PropertyInfo settingProperty, SettingAttribute settingAttribute, string settingConfigVal)
        {
            Type settingType = Type.GetType(settingAttribute.Type);
            if (string.IsNullOrEmpty(settingConfigVal) || settingType == null)
            {
                settingProperty.SetValue(settings, settingAttribute.DefaultValue);
                return;
            }

            if (TryParse(settingConfigVal, settingType, out object settingTypeInstance))
            {
                settingProperty.SetValue(settings, settingTypeInstance);
                return;
            }

            settingProperty.SetValue(settings, settingAttribute.DefaultValue);
        }

        private static bool TryParse<TSetting>(string text, Type settingType, out TSetting res)
        {
            try
            {
                res = (TSetting)Convert.ChangeType(text, settingType);
                return true;
            }
            catch
            {
                res = default(TSetting);
                return false;
            }
        }
    }
}
