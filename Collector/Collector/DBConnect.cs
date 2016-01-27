using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using ReEvents;

namespace Collector
{
    partial class DBConnect
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;
        public List<string[]> rows = new List<string[]>();

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
                        MessageBox.Show("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        MessageBox.Show("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        //public bool CheckExistOld(article item)     //not used
        //{
        //    int num = 0;
        //    if (this.OpenConnection() == true)
        //    {
        //        //create command and assign the query and connection from the constructor
        //        string query = "select count(*) from tb_news where news_title=@title and news_date=@date and type=@type and news_url=@url and company_name=@company_name and sector=@sector and country=@country";
        //        using (var command = new MySqlCommand(query, connection))
        //        {
        //            command.Parameters.Add("@title", MySqlDbType.VarChar).Value = item.title;
        //            command.Parameters.Add("@date", MySqlDbType.VarChar).Value = item.dt;
        //            command.Parameters.Add("@url", MySqlDbType.VarChar).Value = item.href;
        //            command.Parameters.Add("@type", MySqlDbType.VarChar).Value = item.sourceId.ToString("D3");
        //            command.Parameters.Add("@company_name", MySqlDbType.VarChar).Value = item.companyname;
        //            command.Parameters.Add("@sector", MySqlDbType.VarChar).Value = item.sector;
        //            command.Parameters.Add("@country", MySqlDbType.VarChar).Value = item.country;
        //            num = Convert.ToInt32(command.ExecuteScalar());
        //        }

        //        //close connection
        //        this.CloseConnection();
        //    }

        //    if (num == 0)
        //        return false;
        //    return true;
        //}

        public bool CheckExist(article item) 
        {
            int num = 0;
            if (OpenConnection())
            {
                //string query = "select count(*) from tbl_articles where articleurl=@articleurl and articledate=@articledate";
                string query = "select count(*) from tbl_articles where articleurl=@articleurl";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.Add("@articleurl", MySqlDbType.VarChar).Value = item.href;
                    //command.Parameters.Add("@articledate", MySqlDbType.DateTime).Value = item.dt;
                    num = Convert.ToInt32(command.ExecuteScalar());
                }
                if (num == 0)
                {
                    query = "select count(*) from tbl_articles where title=@title and articledate=@articledate";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.Add("@title", MySqlDbType.VarChar).Value = item.title;
                        command.Parameters.Add("@articledate", MySqlDbType.DateTime).Value = item.dt;
                        num = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
                this.CloseConnection();
            }
            return num > 0;
        }

        public bool CheckExistShortCnt(article item)
        {
            int num = 0;
            if (OpenConnection())
            {
                //string query = "select count(*) from tbl_articles where articleurl=@articleurl and articledate=@articledate";
                string query = "select count(*) from tbl_articles where content_short=@content_short";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.Add("@content_short", MySqlDbType.VarChar).Value = item.content_short;
                    num = Convert.ToInt32(command.ExecuteScalar());
                }
                this.CloseConnection();
            }
            return num > 0;
        }

        public void DeleteAllOld(int type) //not used
        {
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                string query = "delete from tb_news where type=@type";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.Add("@type", MySqlDbType.VarChar).Value = type.ToString("D3");
                    command.ExecuteNonQuery();
                }

                //close connection
                this.CloseConnection();
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

        private DateTime GetDBDateTime(string SqlFieldName, MySqlDataReader Reader)
        {
            return Reader[SqlFieldName].Equals(DBNull.Value) ? DateTime.MinValue : Reader.GetDateTime(SqlFieldName);
        }

        private float GetDBFloat(string SqlFieldName, MySqlDataReader Reader)
        {
            return Reader[SqlFieldName].Equals(DBNull.Value) ? -1 : Reader.GetFloat(SqlFieldName);
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
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public DateTime selectLatestDate(int sourceTypeId, int sourceId, int monthes)
        {
            DateTime dt = DateTime.MinValue;
            if (OpenConnection())
            {
                string query = "select max(ArticleDate) as dt from tbl_articles where sourcetypeid='" + sourceTypeId + "' and SourceID='" + sourceId + "'";
                using (var command = new MySqlCommand(query, connection))
                {
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        dt = GetDBDateTime("dt", reader);
                    }
                }
                this.CloseConnection();
            }

            if (dt == DateTime.MinValue)
            {
                dt = DateTime.Now.AddMonths(-monthes);
                if (dt.Year < 2004)
                {
                    dt = new DateTime(2004, 1, 1);
                }
            }

            return dt;
        }


        public List<GraphEventData> tempGetEventDataForGraph(int countryId)
        {
            List<GraphEventData> lst = new List<GraphEventData>();
            if (!OpenConnection())
                return lst;
            string query = "SELECT a.ArticleID, a.Date, a.TagTypeValue, b.ArticleDate FROM tbl_eventdetails as a, tbl_articles as b where a.articleId = b.id and a.CountryID = " + countryId.ToString();
            using (var command = new MySqlCommand(query, connection))
            {
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    GraphEventData rec = new GraphEventData();
                    rec.eventId = GetDBInt("TagTypeValue", reader);
                    rec.articleDate = GetDBDateTime("ArticleDate", reader);
                    rec.date = GetDBString("Date", reader);
                    if (rec.eventId != -1)
                        lst.Add(rec);
                }
            }
            CloseConnection();
            return lst;
        }

        public List<GraphSentimentData> tempGetSentimentDataForGraph(int countryId)
        {
            List<GraphSentimentData> lst = new List<GraphSentimentData>();
            if (!OpenConnection())
                return lst;
            string query = "SELECT a.date, a.value FROM kensee.tbl_market_sentiment as a where a.period = 36 and a.countryId = " + countryId.ToString();
            using (var command = new MySqlCommand(query, connection))
            {
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    GraphSentimentData rec = new GraphSentimentData();
                    rec.sentiment = GetDBFloat("Value", reader);
                    rec.date = GetDBDateTime("Date", reader);
                    lst.Add(rec);
                }
            }
            CloseConnection();
            return lst;
        }

    }
}
