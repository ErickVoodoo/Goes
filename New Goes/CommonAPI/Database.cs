using New_Goes.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Goes.CommonAPI
{
    public class Database
    {
        public static void CreateTables()
        {
            string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");
            SQLiteConnection connection = new SQLiteConnection(dbPath);
            connection.CreateTable<Route>();
            connection.CreateTable<Direction>();
            connection.CreateTable<Stop>();
            connection.CreateTable<StopName>();
            //connection.CreateTable<Pointer>();
            connection.CreateTable<FBusSQL>();
            connection.CreateTable<TaxiSQL>();
        }

        public static void DropDatabase()
        {
            string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");
            SQLiteConnection connection = new SQLiteConnection(dbPath);
            connection.Query<Route>("delete from route");
            connection.Query<Direction>("delete from direction");
            connection.Query<Stop>("delete from stop");
            connection.Query<StopName>("delete from stopname");
            //connection.Query<Pointer>("delete from pointer");
        }

        public static string GetTaxiJSON(string city)
        {
            string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");
            SQLiteConnection connection = new SQLiteConnection(dbPath);
            var taxi = connection.Query<Model.FBusSQL>(String.Format("SELECT * FROM taxisql WHERE city='{0}'", city));

            return taxi.Count == 0 ? null : taxi[0].json;
        }

        public static void AddTaxi(string city, string json)
        {
            string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");
            SQLiteConnection connection = new SQLiteConnection(dbPath);
            connection.Insert(new TaxiSQL() { city = city, json = json });
        }

        public static string GetFBusJSON(string city)
        {
            string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");
            SQLiteConnection connection = new SQLiteConnection(dbPath);
            var fbus = connection.Query<Model.FBusSQL>(String.Format("SELECT * FROM fbussql WHERE city='{0}'", city));

            return fbus.Count == 0 ? null : fbus[0].json;
        }

        public static void AddFbus(string city, string json)
        {
            string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");
            SQLiteConnection connection = new SQLiteConnection(dbPath);
            connection.Insert(new FBusSQL() { city = city, json = json});
        }

        public static void AddOrRemoveFromFavorite(int s_id, int r_id, int d_id)
        {
            string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");
            SQLiteConnection connection = new SQLiteConnection(dbPath);
            if (connection.Query<Model.StopNameSQL>(String.Format("SELECT * FROM stop as s WHERE s.n_id={0} AND s.r_id={1} AND s.d_id={2} AND s.favorite=1", s_id, r_id, d_id)).Count != 0)
            {
                connection.Query<Model.StopNameSQL>(String.Format("UPDATE stop SET favorite=0 WHERE n_id={0} AND r_id={1} AND d_id={2}", s_id, r_id, d_id));
            }
            else
            {
                connection.Query<Model.StopNameSQL>(String.Format("UPDATE stop SET favorite=1 WHERE n_id={0} AND r_id={1} AND d_id={2}", s_id, r_id, d_id));
            }
        }

        public static void AddOrRemoveFromFavoriteWholeStop(string name)
        {
            string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");
            SQLiteConnection connection = new SQLiteConnection(dbPath);

            List<StopNameSQL> stops = connection.Query<Model.StopNameSQL>(String.Format("SELECT * FROM stopname as sn WHERE sn.name LIKE '%{0}%'", name));

            foreach (var stop in stops)
            {
                if (connection.Query<Model.StopNameSQL>(String.Format("SELECT * FROM stop as s WHERE s.n_id={0} AND s.favorite=1", stop.id)).Count ==
                connection.Query<Model.StopNameSQL>(String.Format("SELECT * FROM stop as s WHERE s.n_id={0}", stop.id)).Count)
                {
                    connection.Query<Model.StopNameSQL>(String.Format("UPDATE stop SET favorite=0 WHERE n_id={0}", stop.id));
                }
                else
                {
                    connection.Query<Model.StopNameSQL>(String.Format("UPDATE stop SET favorite=1 WHERE n_id={0}", stop.id));
                }
            }
        }

        public static void RemoveWholeStops(int s_id)
        {
            string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");
            SQLiteConnection connection = new SQLiteConnection(dbPath);
            connection.Query<Model.StopNameSQL>(String.Format("UPDATE stop SET favorite=0 WHERE n_id={0}", s_id));
        }

        public static bool IfAllStopsAreFavorite(int s_id)
        {
            string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");
            SQLiteConnection connection = new SQLiteConnection(dbPath);

            if (connection.Query<Model.StopNameSQL>(String.Format("SELECT * FROM stop as s WHERE s.n_id={0} AND s.favorite=1", s_id)).Count ==
                connection.Query<Model.StopNameSQL>(String.Format("SELECT * FROM stop as s WHERE s.n_id={0}", s_id)).Count)
            {
                return true;
            }
            return false;
        }

        public static List<Stop> GetAllFavoriteStops()
        {
            string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");
            SQLiteConnection connection = new SQLiteConnection(dbPath);
            List<Stop> stops = connection.Query<Stop>("SELECT * FROM stop WHERE favorite=1");
            return stops;
        }
    }
}
