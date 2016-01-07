using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace New_Goes.CommonAPI
{
    public class Schedule
    {
        public async Task<Status> GetSchedule(string city)
        {
            Database.CreateTables();
            string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");

            List<Route> Routes = new List<Route>();
            List<Direction> Directions = new List<Direction>();
            List<Stop> Stops = new List<Stop>();
            List<StopName> StopNames = new List<StopName>();
            List<Pointer> Pointers = new List<Pointer>();

            var values = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(Constant.API_KEY, ""),
            };

            try
            {
                var HttpClientConnection = new HttpClient(new HttpClientHandler());
                HttpResponseMessage ResponseConnection = await HttpClientConnection.PostAsync(city, new FormUrlEncodedContent(values));
                ResponseConnection.EnsureSuccessStatusCode();
                string ResponseString = await ResponseConnection.Content.ReadAsStringAsync();

                JsonObject jsonObject = JsonObject.Parse(ResponseString);
                JsonArray routesArray = jsonObject["routes"].GetArray();
                foreach (JsonValue route in routesArray)
                {
                    JsonObject objectR = route.GetObject();
                    Routes.Add(new Route
                    {
                        id = objectR["id"].GetString(),
                        number = objectR["number"].GetString(),
                        type = objectR["type"].GetString(),
                        ord = objectR["ord"].GetString(),
                    });
                }

                JsonArray directionsArray = jsonObject["directions"].GetArray();
                foreach (JsonValue direction in directionsArray)
                {
                    JsonObject objectD = direction.GetObject();
                    Directions.Add(new Direction
                    {
                        id = objectD["id"].GetString(),
                        name = objectD["name"].GetString(),
                    });
                }

                JsonArray stopNamesArray = jsonObject["stop_names"].GetArray();
                foreach (JsonValue stopname in stopNamesArray)
                {
                    JsonObject objectSN = stopname.GetObject();
                    StopNames.Add(new StopName
                    {
                        id = objectSN["id"].GetString(),
                        name = objectSN["name"].GetString(),
                    });
                }

                JsonArray pointersArray = jsonObject["pointers"].GetArray();
                foreach (JsonValue pointer in pointersArray)
                {
                    JsonObject objectP = pointer.GetObject();
                    Pointers.Add(new Pointer
                    {
                        id = objectP["id"].GetString(),
                        name = objectP["name"].GetString(),
                    });
                }

                JsonArray stopsArray = jsonObject["stops"].GetArray();
                foreach (JsonValue stops in stopsArray)
                {
                    JsonObject objectS = stops.GetObject();
                    Stops.Add(new Stop
                    {
                        id = objectS["id"].GetString(),
                        n_id = objectS["n_id"].GetString(),
                        r_id = objectS["r_id"].GetString(),
                        d_id = objectS["d_id"].GetString(),
                        p_id = objectS["p_id"].GetString(),
                        days = objectS["days"].GetString(),
                        schedule = objectS["schedule"].GetString(),
                    });
                }

                SQLiteConnection connection = new SQLiteConnection(dbPath);
                connection.InsertAll(Routes);
                connection.InsertAll(Directions);
                connection.InsertAll(StopNames);
                connection.InsertAll(Stops);
                connection.InsertAll(Pointers);
            }
            catch (Exception e)
            {
                return new Status(false, e.Message);
            }
            return new Status(true, "Success");
        }
    }

    public class Route
    {
        public string id { get; set; }
        public string number { get; set; }
        public string type { get; set; }
        public string ord { get; set; }
    }

    public class Direction
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class StopName
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Pointer
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Stop
    {
        public string id { get; set; }
        public string n_id { get; set; }
        public string r_id { get; set; }
        public string d_id { get; set; }
        public string p_id { get; set; }
        public string days { get; set; }
        public string schedule { get; set; }
    }

    public class RootObject
    {
        public List<Route> routes { get; set; }
        public List<Direction> directions { get; set; }
        public List<StopName> stop_names { get; set; }
        public List<Pointer> pointers { get; set; }
        public List<Stop> stops { get; set; }
        public int version { get; set; }
        public string cityNiceName { get; set; }
    }

    public class Status
    {
        public Status(bool st, string reas)
        {
            this.isSuccess = st;
            this.reason = reas;
        }

        public bool isSuccess { get; set; }
        public string reason { get; set; }
    }
}
