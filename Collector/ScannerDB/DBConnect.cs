using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace ScannerDB
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

        public void cleanTable(string table)
        {
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                string query = "TRUNCATE kensee." + table;
                using (var command = new MySqlCommand(query, connection))
                {
                    MySqlDataReader reader = command.ExecuteReader();
                }
                this.CloseConnection();
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
        
        public List<MarketSentimentItem> readSentiments(int countryId)
        {
            List<MarketSentimentItem> list = new List<MarketSentimentItem>();
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
                        MarketSentimentItem info = new MarketSentimentItem(
                            GetDBDateTime("ArticleDate", reader),
                            100 * GetDBFloat("Sentiment", reader) / matches.Count,
                            0,
                            0);
                        list.Add(info);
                    }
                }
                this.CloseConnection();
            }
            return list;
        }

        public List<Tag> readTopics()
        {
            List<Tag> topics = new List<Tag>();
            if (this.OpenConnection() == true)
            {
                string query = "select Id, Name from tbl_articletagtypevalues " +
                    "Where tbl_articletagtypevalues.ArticleSubTagTypeId=3";
                using (var command = new MySqlCommand(query, connection))
                {
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Tag info = new Tag(GetDBInt("Id", reader), GetDBString("Name", reader));
                        topics.Add(info);
                    }
                }
                this.CloseConnection();
            }
            return topics;
        }

        public List<Tag> readProperties()
        {
            List<Tag> properties = new List<Tag>();
            if (this.OpenConnection() == true)
            {
                string query = "select Id, Name from tbl_articletagtypevalues " +
                    "Where tbl_articletagtypevalues.ArticleSubTagTypeId=1 OR tbl_articletagtypevalues.ArticleSubTagTypeId=2";
                using (var command = new MySqlCommand(query, connection))
                {
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Tag info = new Tag(GetDBInt("Id", reader), GetDBString("Name", reader));
                        properties.Add(info);
                    }
                }
                this.CloseConnection();
            }
            return properties;
        }

        public List<HighlightsItem> readArticles(int countryId, int topicId, DateTime startDate)
        {
            List<HighlightsItem> list = new List<HighlightsItem>();
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                string query = "select ArticleDate, Title, ArticleURL from tbl_articles " +
                    "join tbl_articlelocations on tbl_articles.ID = tbl_articlelocations.ArticleID AND tbl_articlelocations.LocationTypeID=2 " +
                    "join tbl_articletags on tbl_articletags.ArticleID = tbl_articles.ID AND tbl_articletags.ArticleTagTypeValue=@topicId " +
                    "Where tbl_articletags.ArticleID=tbl_articles.ID AND tbl_articletags.ArticleTagTypeValue=@topicId " +
                    "AND tbl_articlelocations.LocationTypeID=2 AND tbl_articlelocations.LocationId=@locationId AND ArticleDate>@date " +
                    "ORDER BY tbl_articles.ArticleDate DESC";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.Add("@locationId", MySqlDbType.Int16).Value = countryId;
                    command.Parameters.Add("@topicId", MySqlDbType.Int16).Value = topicId;
                    command.Parameters.Add("@date", MySqlDbType.DateTime).Value = startDate;
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        HighlightsItem info = new HighlightsItem(GetDBDateTime("ArticleDate", reader), GetDBString("Title", reader), GetDBString("ArticleURL", reader));
                        list.Add(info);
                    }
                }
                this.CloseConnection();
            }
            return list;
        }

        public Dictionary<string, List<ComparisionDataItem>> readArticlesForProperty(int countryId)
        {
            Dictionary<string, List<ComparisionDataItem>> list = new Dictionary<string, List<ComparisionDataItem>>();
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                string query = "select tbl_articles.ID, ArticleDate, LocationTypeID, tbl_articletagtypes.Name type_name, tbl_articletagtypevalues.Name value_name from tbl_articles " +
                    "join tbl_articlelocations on tbl_articles.ID = tbl_articlelocations.ArticleID AND tbl_articlelocations.LocationTypeID=2 " +
                    "join tbl_articletags on tbl_articletags.ArticleID = tbl_articles.ID " +
                    "join tbl_articletagtypevalues on tbl_articletagtypevalues.ID = tbl_articletags.ArticleTagTypeValue " +
                    "join tbl_articlesubtagtype on tbl_articlesubtagtype.ID = tbl_articletagtypevalues.ArticleSubTagTypeID " +
                    "join tbl_articletagtypes on tbl_articletagtypes.ID = tbl_articlesubtagtype.ArticleTagTypeID " +
                    "Where tbl_articlelocations.LocationTypeID=2 AND tbl_articlelocations.LocationId=@locationId " +
                    "ORDER BY tbl_articles.ID";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.Add("@locationId", MySqlDbType.Int16).Value = countryId;
                    MySqlDataReader reader = command.ExecuteReader();
                    Dictionary<string, int> topicDic = new Dictionary<string, int>();
                    Dictionary<string, int> propertyDic = new Dictionary<string, int>();
                    int articleId = -1;
                    ComparisionDataItem item = new ComparisionDataItem { buy_sell_count = 0, construction_count = 0, date = DateTime.MinValue, rent_count = 0 };
                    while (reader.Read())
                    {
                        var id = GetDBInt("ID", reader);
                        if (articleId == -1 || articleId != id)
                        {
                            if (articleId != -1)
                            {
                                foreach (var x in propertyDic)
                                {
                                    if (!list.ContainsKey(x.Key))
                                        list.Add(x.Key, new List<ComparisionDataItem>());
                                    list[x.Key].Add(item);
                                }
                            }
                            topicDic.Clear();
                            propertyDic.Clear();
                            item = new ComparisionDataItem { buy_sell_count = 0, construction_count = 0, date = GetDBDateTime("ArticleDate", reader), rent_count = 0 };
                            articleId = id;
                        }
                        var type_name = GetDBString("type_name", reader);
                        var value_name = GetDBString("value_name", reader);
                        if (type_name == "Topic")
                        {
                            if (!topicDic.ContainsKey(value_name))
                            {
                                topicDic.Add(value_name, 0);
                                switch (value_name)
                                {
                                    case "Buy/Sell":
                                        item.buy_sell_count++;
                                        break;
                                    case "Construction":
                                        item.construction_count++;
                                        break;
                                    case "Rent":
                                        item.rent_count++;
                                        break;
                                }
                            }
                        }
                        else if (type_name == "Property")
                        {
                            if (!propertyDic.ContainsKey(value_name))
                            {
                                propertyDic.Add(value_name, 0);
                            }
                        }
                    }
                    foreach (var x in propertyDic)
                    {
                        if (!list.ContainsKey(x.Key))
                            list.Add(x.Key, new List<ComparisionDataItem>());
                        list[x.Key].Add(item);
                    }
                }
                this.CloseConnection();
            }
            return list;
        }

        public void storeSentimentToDB(int countryId, int period, DateTime date, float value, float newResidentialConstruction, float nationalHomePriceIndex)
        {
            if (OpenConnection())
            {
                string query = "INSERT INTO tbl_market_sentiment (CountryId, Date, Value, NewResidentialConstruction, NationalHomePriceIndex, Period) " +
                        "VALUES(@countryId, @date, @value, @newResidentialConstruction,@NationalHomePriceIndex,  @period)";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.Add("@countryId", MySqlDbType.Int16).Value = countryId;
                    command.Parameters.Add("@date", MySqlDbType.DateTime).Value = date;
                    command.Parameters.Add("@value", MySqlDbType.Float).Value = value;
                    command.Parameters.Add("@newResidentialConstruction", MySqlDbType.Float).Value = newResidentialConstruction;
                    command.Parameters.Add("@NationalHomePriceIndex", MySqlDbType.Float).Value = nationalHomePriceIndex;
                    command.Parameters.Add("@period", MySqlDbType.Int16).Value = period;
                    command.ExecuteNonQuery();
                }
                CloseConnection();
            }
        }

        public void storeSentimentsToDB(List<MarketSentimentItem> msl, int countryId, int period)
        {
            if (OpenConnection())
            {
                foreach (var x in msl)
                {
                    string query = "INSERT INTO tbl_market_sentiment (CountryId, Date, Value, NewResidentialConstruction, NationalHomePriceIndex, Period) " +
                            "VALUES(@countryId, @date, @value, @newResidentialConstruction,@NationalHomePriceIndex,  @period)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.Add("@countryId", MySqlDbType.Int16).Value = countryId;
                        command.Parameters.Add("@date", MySqlDbType.DateTime).Value = x.date;
                        command.Parameters.Add("@value", MySqlDbType.Float).Value = x.sentimemt_index_period_smoothed;
                        command.Parameters.Add("@newResidentialConstruction", MySqlDbType.Float).Value = x.newResidentialConstruction;
                        command.Parameters.Add("@NationalHomePriceIndex", MySqlDbType.Float).Value = x.nationalHomePriceIndex;
                        command.Parameters.Add("@period", MySqlDbType.Int16).Value = period;
                        command.ExecuteNonQuery();
                    }
                }
                CloseConnection();
            }
        }

        public void storeHighlightsToDB(List<HighlightsItem> hls, int countryId, string topicName)
        {
            if (OpenConnection())
            {
                foreach (var x in hls)
                {
                    string query = "INSERT INTO tbl_highlights (topic, title, date, countryId, url) " +
                            "VALUES(@topic, @title, @date, @countryId, @url)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.Add("@topic", MySqlDbType.VarChar).Value = topicName;
                        command.Parameters.Add("@title", MySqlDbType.VarChar).Value = x.title;
                        command.Parameters.Add("@date", MySqlDbType.DateTime).Value = x.date;
                        command.Parameters.Add("@url", MySqlDbType.VarChar).Value = x.url;
                        command.Parameters.Add("@countryId", MySqlDbType.Int16).Value = countryId;
                        command.ExecuteNonQuery();
                    }
                }
                CloseConnection();
            }
        }

        public void storeComparisionDataToDB(List<ComparisionDataItem> hls, int countryId, string propertyName)
        {
            if (OpenConnection())
            {
                foreach (var x in hls)
                {
                    string query = "INSERT INTO tbl_comparision_data (Property, Date, CountryId, BuySell, Construction, Rent) " +
                            "VALUES(@property, @date, @countryId, @buySell, @construction, @rent)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.Add("@property", MySqlDbType.VarChar).Value = propertyName;
                        command.Parameters.Add("@date", MySqlDbType.DateTime).Value = x.date;
                        command.Parameters.Add("@countryId", MySqlDbType.Int16).Value = countryId;
                        command.Parameters.Add("@buySell", MySqlDbType.Int16).Value = x.buy_sell_count;
                        command.Parameters.Add("@construction", MySqlDbType.Int16).Value = x.construction_count;
                        command.Parameters.Add("@rent", MySqlDbType.Int16).Value = x.rent_count;
                        command.ExecuteNonQuery();
                    }
                }
                CloseConnection();
            }
        }

        public void storeMacroToDB(Dictionary<DateTime, MacroItem> dict, int countryId)
        {
            if (OpenConnection())
            {
                foreach (var x in dict)
                {
                    DateTime dt = x.Key;
                    MacroItem m = x.Value;
                    string query = "INSERT INTO tbl_macro (UnemploymentRate, Date, CountryId, Inflation,HousePriceIndexChange,RateForLodging,RateForOffice,RateForCommercial,RateForHealthcare,RateForLeasure,RateForNonResidential,RateForResidential) " +
                            "VALUES(@UnemploymentRate, @date, @countryId, @Inflation,@HousePriceIndexChange,@RateForLodging,@RateForOffice,@RateForCommercial,@RateForHealthcare,@RateForLeasure,@RateForNonResidential,@RateForResidential)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.Add("@UnemploymentRate", MySqlDbType.Float).Value = m.UnemploymentRate;
                        command.Parameters.Add("@date", MySqlDbType.DateTime).Value = dt;
                        command.Parameters.Add("@countryId", MySqlDbType.Int16).Value = countryId;
                        command.Parameters.Add("@Inflation", MySqlDbType.Float).Value = m.Inflation;
                        command.Parameters.Add("@HousePriceIndexChange", MySqlDbType.Float).Value = m.HousePriceIndexChange;
                        command.Parameters.Add("@RateForLodging", MySqlDbType.Float).Value = m.RateForLodging;
                        command.Parameters.Add("@RateForOffice", MySqlDbType.Float).Value = m.RateForOffice;
                        command.Parameters.Add("@RateForCommercial", MySqlDbType.Float).Value = m.RateForCommercial;
                        command.Parameters.Add("@RateForHealthcare", MySqlDbType.Float).Value = m.RateForHealthcare;
                        command.Parameters.Add("@RateForLeasure", MySqlDbType.Float).Value = m.RateForLeasure;
                        command.Parameters.Add("@RateForNonResidential", MySqlDbType.Float).Value = m.RateForNonResidential;
                        command.Parameters.Add("@RateForResidential", MySqlDbType.Float).Value = m.RateForResidential;
                        command.ExecuteNonQuery();
                    }
                }
                CloseConnection();
            }
        }

        public void storeGeneralInformationToDB(string name, string value, int countryId)
        {
            if (OpenConnection())
            {
                string query = "INSERT INTO tbl_general_information (CountryId, Name, Value) " +
                      "VALUES(@countryId, @name, @value)";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.Add("@countryId", MySqlDbType.Int16).Value = countryId;
                    command.Parameters.Add("@name", MySqlDbType.VarChar).Value = name;
                    command.Parameters.Add("@value", MySqlDbType.VarChar).Value = value;
                    command.ExecuteNonQuery();
                }
                CloseConnection();
            }
        }

        public List<WebSiteInfo> readNewsWebsite()
        {
            List<WebSiteInfo> list = new List<WebSiteInfo>();
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                string query = "select * from tbl_newswebsites";
                using (var command = new MySqlCommand(query, connection))
                {
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        WebSiteInfo info = new WebSiteInfo(GetDBString("URLAddress", reader),
                            GetDBInt("Id", reader),
                            GetDBString("Name", reader),
                            GetDBString("ScrapingRule", reader),
                            GetDBString("PagingRule", reader),
                            GetDBInt("IsActive", reader),
                            GetDBInt("countryId", reader),
                            GetDBInt("Period", reader),
                            GetDBInt("BuildSentiments", reader));
                        string urlRule = GetDBString("UrlRule", reader);
                        var x = urlRule.Split(new char[] { ';' });
                        info.baseUrl = x[0].Split(new char[] { ',' });
                        if (x.Length == 2)
                            info.filterUrl = x[1].Split(new char[] { ',' });
                        list.Add(info);
                    }
                }
                this.CloseConnection();
            }
            return list;
        }
    }
}
