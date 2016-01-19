using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;

namespace New_Goes.CommonAPI
{
    public class Constant
    {
        public static string API_KEY = "api_private_key_abc_hren_product_goda_2015";

        public static string CITY_GRODNO_SCHEDULE   = "http://grodno.btrans.by/dev/api/v3/schedule";
        public static string CITY_GRODNO_METADATA   = "http://grodno.btrans.by/dev/api/v3/metadata";
        public static string CITY_BREST_SCHEDULE    = "http://brest.btrans.by/dev/api/v3/schedule";
        public static string CITY_BREST_METADATA    = "http://brest.btrans.by/dev/api/v3/metadata";
        public static string CITY_MOGILEV_SCHEDULE  = "http://mogilev.btrans.by/dev/api/v3/schedule";
        public static string CITY_MOGILEV_METADATA  = "http://mogilev.btrans.by/dev/api/v3/metadata";
        public static string CITY_VITEBSK_SCHEDULE  = "http://vitebsk.btrans.by/dev/api/v3/schedule";
        public static string CITY_VITEBSK_METADATA  = "http://vitebsk.btrans.by/dev/api/v3/metadata";
        public static string CITY_GOMEL_SCHEDULE    = "http://gomel.btrans.by/dev/api/v3/schedule";
        public static string CITY_GOMEL_METADATA    = "http://gomel.btrans.by/dev/api/v3/metadata";
        public static string CITY_MINSK_SCHEDULE    = "http://minsk.btrans.by/dev/api/v3/schedule";
        public static string CITY_MINSK_METADATA    = "http://minsk.btrans.by/dev/api/v3/metadata";
        
        public static StaticData[] City = {     new StaticData("Брест", "brest", CITY_BREST_SCHEDULE, "Transparent", "White"),
                                                new StaticData("Гродно", "grodno", CITY_GRODNO_SCHEDULE, "Transparent", "White"),
                                                new StaticData("Гомель", "gomel", CITY_GOMEL_SCHEDULE, "Transparent", "White"),
                                                new StaticData("Витебск", "vitebsk", CITY_VITEBSK_SCHEDULE, "Transparent", "White"),
                                                new StaticData("Минск", "minsk", CITY_MINSK_SCHEDULE, "Transparent", "White"),
                                                new StaticData("Могилев", "mogilev", CITY_MOGILEV_SCHEDULE, "Transparent", "White")};

        public static string FavoriteStar = "SolidStar";
        public static string UnFavoriteStar = "OutlineStar";

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
                await Task.Delay(3000);
                await progressbar.HideAsync();
            }
        }
    }

    public class StaticData {
        public StaticData(string title, string key, string value, string background, string foreground)
        {
            this.key = key;
            this.value = value;
            this.title = title;
            this.background = background;
            this.foreground = foreground;
        }

        public string title { get; set; }
        public string key { get; set;}
        public string value { get; set;}
        public string background { get; set;}
        public string foreground { get; set;}
    }
}
