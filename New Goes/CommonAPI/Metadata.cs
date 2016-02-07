using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace New_Goes.CommonAPI
{
    public class Metadata
    {
        public static async Task<StatusMetaData> Get_MetaData()
        {
            RootObject root_obj = new RootObject(); 
            var values = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(Constant.API_KEY, ""),
            };

            try
            {
                var HttpClientConnection = new HttpClient(new HttpClientHandler());
                HttpResponseMessage ResponseConnection = await HttpClientConnection.PostAsync("https://" + LocalProperties.LoadFromToLP(LocalProperties.LP_selected_city) + ".btrans.by/dev/api/v3/metadata", new FormUrlEncodedContent(values));
                ResponseConnection.EnsureSuccessStatusCode();
                string ResponseString = await ResponseConnection.Content.ReadAsStringAsync();

                JObject obj = JObject.Parse(ResponseString);
                root_obj = JsonConvert.DeserializeObject<RootObject>(obj.ToString());
            }
            catch (Exception e)
            {
                return new StatusMetaData(false, e.Message, null);
            }
           return new StatusMetaData(true, "Success", root_obj);
        }

        public class CitiesVersions
        {
            public int brest { get; set; }
            public int vitebsk { get; set; }
            public int grodno { get; set; }
            public int minsk { get; set; }
            public int gomel { get; set; }
            public int mogilev { get; set; }
        }

        public class AndroidVersions
        {
            public int latest { get; set; }
            public int critical { get; set; }
        }

        public class RootObject
        {
            public CitiesVersions cities_versions { get; set; }
            public AndroidVersions android_versions { get; set; }
        }

        public class StatusMetaData
        {
            public StatusMetaData(bool st, string reas, RootObject obj )
            {
                this.isSuccess = st;
                this.reason = reas;
                this.obj = obj;
            }

            public bool isSuccess { get; set; }
            public string reason { get; set; }
            public RootObject obj { get; set; }
        }
    }
}
