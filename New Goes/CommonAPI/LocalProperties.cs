using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Goes.CommonAPI
{
    public class LocalProperties
    {
        public static void SaveToLP(string key, string value)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[key] = value;
        }

        public static string LoadFromToLP(string key)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            return localSettings.Values[key] == null ? null : localSettings.Values[key].ToString();
        }

        public static void RemoveFrom(string key)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values.Remove(key);
        }

        public static string LP_theme = "theme";
        public static string theme_light = "light";
        public static string theme_dark = "dark";

        public static string LP_selected_city = "selected_city";
        public static string LP_current_version = "current_version";

        public static string LP_active_premium = "active_premium";
    }
}
