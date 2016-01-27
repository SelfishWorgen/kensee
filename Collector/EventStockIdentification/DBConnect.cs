using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace EventStockIdentification
{
    public class EventInfo
    {
        public string company;
        public DateTime dt;
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
        
        public DateTime readDateTime(int articleId)
        {
            DateTime dt = DateTime.MinValue;
            if (this.OpenConnection() == true)
            {
                string query = "select ArticleDate from tbl_articles Where Id=@articleId";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.Add("@articleId", MySqlDbType.Int16).Value = articleId;
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        dt = GetDBDateTime("ArticleDate", reader);
                        dt = new DateTime(dt.Year, dt.Month, dt.Day);
                    }
                }
                this.CloseConnection();
            }
            return dt;
        }

        public int readCompanyId(string company)
        {
            int id = 0;
            if (this.OpenConnection() == true)
            {
                string query = "select ID from tbl_recompanies Where Name=@company";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.Add("@company", MySqlDbType.VarChar).Value = company;
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        id = GetDBInt("ID", reader);
                    }
                }
                this.CloseConnection();
            }
            return id;
        }

        public List<int> readArticleIdsForEvent(int eventId)
        {
            List<int> res = new List<int>();
            if (this.OpenConnection() == true)
            {
                string query = "select ArticleId from tbl_articletags Where ArticleTagTypeValue=@eventId";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.Add("@eventId", MySqlDbType.Int16).Value = eventId;
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(GetDBInt("ArticleId", reader));
                    }
                }
                this.CloseConnection();
            }
            return res;
        }

        public List<int> readArticleIdsForCompany(int companyId)
        {
            List<int> res = new List<int>();
            if (this.OpenConnection() == true)
            {
                string query = "select ArticleId from tbl_articlecompanies Where RECompanyID=@companyId";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.Add("@companyId", MySqlDbType.Int16).Value = companyId;
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(GetDBInt("ArticleId", reader));
                    }
                }
                this.CloseConnection();
            }
            return res;
        }
    }
}
