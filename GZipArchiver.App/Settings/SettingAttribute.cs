using System;

namespace GZipArchiver.App.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SettingAttribute : Attribute
    {
        public string Key { get; set; }

        public object DefaultValue { get; set; }

        public string Type { get; set; }
    }
}
