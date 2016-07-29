using System;
using System.Configuration;
using System.Linq;

namespace Utils
{
    public static class AppSettings
    {
        public static bool GetBoolean(string key, bool defaultValue = false)
        {
            var value = GetValue(key);
            bool.TryParse(value, out defaultValue);

            return defaultValue;
        }

        public static int GetInt32(string key, int defaultValue = 0)
        {
            var value = GetValue(key);
            int.TryParse(value, out defaultValue);

            return defaultValue;
        }

        public static double GetDouble(string key, double defaultValue = 0)
        {
            var value = GetValue(key);
            double.TryParse(value, out defaultValue);

            return defaultValue;
        }

        public static string GetString(string key, string defaultValue = "")
        {
            var value = GetValue(key);
            if (!string.IsNullOrEmpty(value))
                return value;

            return defaultValue;
        }

        public static DateTime? GetDateTime(string key, DateTime? defaultValue = null)
        {
            var value = GetValue(key);
            DateTime dateTime;
            if (DateTime.TryParse(value, out dateTime))
                return dateTime;

            return defaultValue;
        }

        private static string GetValue(string key)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
                return ConfigurationManager.AppSettings[key];

            return string.Empty;
        }
    }
}
