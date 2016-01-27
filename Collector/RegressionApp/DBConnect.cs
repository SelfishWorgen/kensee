using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace RegressionApp
{
    public class WebSiteInfo
    {
        public WebSiteInfo(string u, int n, string nm, string r, string pr, int ia, int ci, int pi, int bs)
        {
            url = u; name = nm; id = n; rule = r; pagingRule = pr; isActive = ia; countryId = ci; period = pi; buildSentiments = bs;
        }
        public string url;
        public string name;
        public int id;
        public string rule;
        public string pagingRule;
        public int isActive;
        public int buildSentiments;
        public string[] baseUrl;
        public string[] filterUrl;
        public int countryId;
        public int period;
    }

    partial class DBConnect
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        //Constructor
        public DBConnect(string dbName)
        {
            database = dbName;
            Initialize();
        }

        //Initialize values
        private void Initialize()
        {
            server = "localhost";
            uid = "root";
            password = "test";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);
        }

        //open connection to database
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        //               MessageBox.Show("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        //               MessageBox.Show("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        private string GetDBString(string SqlFieldName, MySqlDataReader Reader)
        {
            return Reader[SqlFieldName].Equals(DBNull.Value) ? String.Empty : Reader.GetString(SqlFieldName);
        }

        private int GetDBInt(string SqlFieldName, MySqlDataReader Reader)
        {
            return Reader[SqlFieldName].Equals(DBNull.Value) ? -1 : Reader.GetInt32(SqlFieldName);
        }

        private float GetDBFloat(string SqlFieldName, MySqlDataReader Reader)
        {
            return Reader[SqlFieldName].Equals(DBNull.Value) ? -1 : Reader.GetFloat(SqlFieldName);
        }

        private DateTime GetDBDateTime(string SqlFieldName, MySqlDataReader Reader)
        {
            return Reader[SqlFieldName].Equals(DBNull.Value) ? DateTime.MinValue : Reader.GetDateTime(SqlFieldName);
        }

        //Close connection
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                return false;
            }
        }

        public int getCountryId(string countryName)
        {
            int countryId = -1;
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                string query = "select ID from tbl_countries Where tbl_countries.Name=@countryName";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.Add("@countryName", MySqlDbType.VarChar).Value = countryName;
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        countryId = GetDBInt("ID", reader);
                        break;
                    }
                }
                this.CloseConnection();
            }

            return countryId;
        }

        public List<DateValueItem> readSentiments(int countryId)
        {
            List<DateValueItem> list = new List<DateValueItem>();
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                string query = "select ArticleDate, Sentiment, Content from tbl_articles " +
                    "left join tbl_articlelocations on tbl_articles.ID = tbl_articlelocations.ArticleID " +
                    "Where tbl_articlelocations.LocationTypeID=2 AND tbl_articlelocations.LocationId=@locationId " +
                    "ORDER BY tbl_articles.ArticleDate";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.Add("@locationId", MySqlDbType.Int16).Value = countryId;
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string content = GetDBString("Content", reader);
                        Regex regex = new Regex(@"\w+");
                        MatchCollection matches = regex.Matches(content);
                        DateValueItem info = new DateValueItem(
                            GetDBDateTime("ArticleDate", reader),
                            100 * GetDBFloat("Sentiment", reader) / matches.Count);
                        list.Add(info);
                    }
                }
                this.CloseConnection();
            }
            return list;
        }
    }

       public class DateValueItem
       {
           public DateValueItem(DateTime dt, double vl) { date = dt; value = vl; }
           public DateTime date;
           public double value;
           public double sentimemt_index_period_smoothed;
       }

       public class InfoForSentimentGraph
       {
           public int firstYear;
           public List<List<double>> values;

           public InfoForSentimentGraph() { firstYear = 0; values = new List<List<double>>(); }
       }
}
