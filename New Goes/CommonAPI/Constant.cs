using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

namespace New_Goes.CommonAPI
{
    public class Constant
    {
        public static string API_KEY = "api_private_key_abc_hren_product_goda_2015";

        public static string PREMIUM_API_KEY = "goes_premium_2016";

        public static string CITY_GRODNO_SCHEDULE   = "https://grodno.btrans.by/dev/api/v3/schedule";
        public static string CITY_GRODNO_METADATA   = "https://grodno.btrans.by/dev/api/v3/metadata";
        public static string CITY_BREST_SCHEDULE    = "https://brest.btrans.by/dev/api/v3/schedule";
        public static string CITY_BREST_METADATA    = "https://brest.btrans.by/dev/api/v3/metadata";
        public static string CITY_MOGILEV_SCHEDULE  = "https://mogilev.btrans.by/dev/api/v3/schedule";
        public static string CITY_MOGILEV_METADATA  = "https://mogilev.btrans.by/dev/api/v3/metadata";
        public static string CITY_VITEBSK_SCHEDULE  = "https://vitebsk.btrans.by/dev/api/v3/schedule";
        public static string CITY_VITEBSK_METADATA  = "https://vitebsk.btrans.by/dev/api/v3/metadata";
        public static string CITY_GOMEL_SCHEDULE    = "https://gomel.btrans.by/dev/api/v3/schedule";
        public static string CITY_GOMEL_METADATA    = "https://gomel.btrans.by/dev/api/v3/metadata";
        public static string CITY_MINSK_SCHEDULE    = "https://minsk.btrans.by/dev/api/v3/schedule";
        public static string CITY_MINSK_METADATA    = "https://minsk.btrans.by/dev/api/v3/metadata";

        public static string CITY_FBUSES            = "http://vkcheck.hol.es/goes/getFbus";
        public static string CITY_TAXIS             = "http://vkcheck.hol.es/goes/getTaxi";

        public static StaticData[] City = {     new StaticData("Брест", "brest", CITY_BREST_SCHEDULE, "Transparent", "White"),
                                                new StaticData("Гродно", "grodno", CITY_GRODNO_SCHEDULE, "Transparent", "White"),
                                                new StaticData("Гомель", "gomel", CITY_GOMEL_SCHEDULE, "Transparent", "White"),
                                                new StaticData("Витебск", "vitebsk", CITY_VITEBSK_SCHEDULE, "Transparent", "White"),
                                                new StaticData("Минск", "minsk", CITY_MINSK_SCHEDULE, "Transparent", "White"),
                                                new StaticData("Могилев", "mogilev", CITY_MOGILEV_SCHEDULE, "Transparent", "White")};

        public static StaticFBusesData[] CityBuses = {
                                                   new StaticFBusesData("Брест", "brest", CITY_FBUSES, null),
                                                   new StaticFBusesData("Витебск", "vitebsk", CITY_FBUSES, null),
                                                   new StaticFBusesData("Гродно", "grodno", CITY_FBUSES, null),
                                                   new StaticFBusesData("Гомель", "gomel", CITY_FBUSES, null),
                                                   new StaticFBusesData("Минск", "minsk", CITY_FBUSES, null),
                                                   new StaticFBusesData("Могилев", "mogilev", CITY_FBUSES, null),
                                               };

        public static StaticFBusesData[] CityTaxis = {
                                                   new StaticFBusesData("Брест", "brest", CITY_TAXIS, null),
                                                   new StaticFBusesData("Витебск", "vitebsk", CITY_TAXIS, null),
                                                   new StaticFBusesData("Гродно", "grodno", CITY_TAXIS, null),
                                                   new StaticFBusesData("Гомель", "gomel", CITY_TAXIS, null),
                                                   new StaticFBusesData("Минск", "minsk", CITY_TAXIS, null),
                                                   new StaticFBusesData("Могилев", "mogilev", CITY_TAXIS, null),
                                               };

        public static string FavoriteStar = "SolidStar";
        public static string UnFavoriteStar = "OutlineStar";

        public static string[] TransportColors = { "#FFBC00", "#3700EB", "#EB0000" };

        public static async void Loader(string loaderMessage, bool showProgress)
        {
            StatusBarProgressIndicator progressbar = StatusBar.GetForCurrentView().ProgressIndicator;
            progressbar.Text = string.IsNullOrEmpty(loaderMessage) ? "Please wait..." : loaderMessage;
            
            if (showProgress)
            {
                await progressbar.ShowAsync();
            }
            else
            {
                await progressbar.HideAsync();
            }
        }

        public async static Task<string> GetJsonFromURI(string uri, List<KeyValuePair<string, string>> data)
        {
            string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");
            var HttpClientConnection = new HttpClient(new HttpClientHandler());
            HttpResponseMessage ResponseConnection = await HttpClientConnection.PostAsync(uri, new FormUrlEncodedContent(data));
            ResponseConnection.EnsureSuccessStatusCode();
            return await ResponseConnection.Content.ReadAsStringAsync();
        }

        public static void ShowNotification(string text) {
            ToastTemplateType toastTemplate = ToastTemplateType.ToastImageAndText01;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(text));

            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("duration", "short");

            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }

    public class StaticFBusesData {
        public StaticFBusesData(string title, string key, string value,  Page page)
        {
            this.title = title;
            this.key = key;
            this.value = value;
            this.page = page;
        }

        public string title { get; set; }
        public string key { get; set;}
        public string value { get; set;}
        public Page page { get; set;}
    }

    public class StaticData
    {
        public StaticData(string title, string key, string value, string background, string foreground)
        {
            this.key = key;
            this.value = value;
            this.title = title;
            this.background = background;
            this.foreground = foreground;
        }

        public string title { get; set; }
        public string key { get; set; }
        public string value { get; set; }
        public string background { get; set; }
        public string foreground { get; set; }
    }
}
