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
            SQLiteAsyncConnection connection = new SQLiteAsyncConnection(dbPath);
            connection.CreateTableAsync<Route>();
            connection.CreateTableAsync<Direction>();
            connection.CreateTableAsync<Stop>();
            connection.CreateTableAsync<StopName>();
            connection.CreateTableAsync<Pointer>();
            connection.CreateTableAsync<FBusSQL>();
            connection.CreateTableAsync<TaxiSQL>();
        }

        public static void DropDatabase()
        {
            string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");
            SQLiteAsyncConnection connection = new SQLiteAsyncConnection(dbPath);
            connection.QueryAsync<Route>("DROP TABLE IF EXISTS route");
            connection.QueryAsync<Direction>("DROP TABLE IF EXISTS direction");
            connection.QueryAsync<Stop>("DROP TABLE IF EXISTS stop");
            connection.QueryAsync<StopName>("DROP TABLE IF EXISTS stopname");
            connection.QueryAsync<Pointer>("DROP TABLE IF EXISTS pointer");
            CreateTables();
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

        public static void AddOrRemoveFromFavoriteWholeStop(int s_id)
        {
            string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");
            SQLiteConnection connection = new SQLiteConnection(dbPath);
            if (connection.Query<Model.StopNameSQL>(String.Format("SELECT * FROM stop as s WHERE s.n_id={0} AND s.favorite=1", s_id)).Count ==
                connection.Query<Model.StopNameSQL>(String.Format("SELECT * FROM stop as s WHERE s.n_id={0}", s_id)).Count)
            {
                connection.Query<Model.StopNameSQL>(String.Format("UPDATE stop SET favorite=0 WHERE n_id={0}", s_id));
            }
            else
            {
                connection.Query<Model.StopNameSQL>(String.Format("UPDATE stop SET favorite=1 WHERE n_id={0}", s_id));
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
    }
}
