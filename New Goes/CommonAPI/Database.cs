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
        }

        public static void DropDatabase()
        {
            string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");
            SQLiteAsyncConnection connection = new SQLiteAsyncConnection(dbPath);
            connection.QueryAsync<Route>("DELETE FROM route");
            connection.QueryAsync<Direction>("DELETE FROM direction");
            connection.QueryAsync<Stop>("DELETE FROM stop");
            connection.QueryAsync<StopName>("DELETE FROM stopname");
            connection.QueryAsync<Pointer>("DELETE FROM pointer");
        }
    }
}
