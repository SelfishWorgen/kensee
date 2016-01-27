using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using KenseeAPI.Models;
using System.Globalization;

namespace KenseeAPI.Controllers
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
                        break;

                    case 1045:
                        break;
                }
                return false;
            }
        }

        public List<Article> getArticles(string text)
        {
            List<Article> articles = new List<Article>();
            try
            {
                //using (System.IO.StreamWriter file = new System.IO.StreamWriter("c:/tmp/a.txt"))
                //{
                //    file.WriteLine("enter to getArticles");
                //    file.WriteLine(DateTime.Now.ToString());
                if (this.OpenConnection() == true)
                {
                    DateTime startTime = DateTime.Now.AddYears(-1);
                    string query;
                    if (text != null)
                        query = "usp_get_articles_context";
                    else
                        query = "usp_get_articles";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.CommandTimeout = 120;
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("dt", startTime);
                        if (text != null)
                            command.Parameters.AddWithValue("cntxt", text);
                        MySqlDataReader reader = command.ExecuteReader();
                        //file.WriteLine("end of Select request"); 
                        //file.WriteLine(DateTime.Now.ToString());
                        Article at = new Article { article_id = -1, topic = "", property = "", city = "", country = "" };
                        Dictionary<string, int> topicDic = new Dictionary<string, int>();
                        Dictionary<string, int> propertyDic = new Dictionary<string, int>();
                        Dictionary<string, int> countryDic = new Dictionary<string, int>();
                        Dictionary<string, int> cityDic = new Dictionary<string, int>();
                        Dictionary<string, int> companyDic = new Dictionary<string, int>();
                        while (reader.Read())
                        {
                            DateTime dt = GetDBTime("ArticleDate", reader);
                            if (dt < startTime)
                                continue;
                            var id = GetDBInt("ID", reader);
                            var sourceTypeId = GetDBInt("SourceTypeId", reader);
                            var LocationTypeID = GetDBInt("LocationTypeID", reader);
                            if (at.article_id == -1 || at.article_id != id)
                            {
                                topicDic.Clear();
                                propertyDic.Clear();
                                countryDic.Clear();
                                cityDic.Clear();
                                companyDic.Clear();
                                if (at.article_id != -1)
                                    articles.Add(at);
                                var title = GetDBString("Title", reader);
                                var snippet = GetDBString("Snippet", reader);
                                string date = dt.ToString("dd-MM-yyyy");
                                var url = GetDBString("ArticleURL", reader);
                                var sentiment = GetDBString("sentiment", reader);
                                var website_name = GetDBString("website_name", reader);
                                var website_name1 = GetDBString("website_name1", reader);
                                at = new Article
                                {
                                    article_id = id,
                                    date = date,
                                    sentiment = sentiment,
                                    source = sourceTypeId == 1 ? website_name : website_name1,
                                    url = url,
                                    title = title,
                                    snippet = snippet,
                                    topic = "",
                                    company="",
                                    property = "", city = "", country = ""
                                };
                            }
                            var type_name = GetDBString("type_name", reader);
                            var value_name = GetDBString("value_name", reader);
                            if (type_name == "Topic")
                            {
                                if (!topicDic.ContainsKey(value_name))
                                {
                                    topicDic.Add(value_name, 0);
                                    if (!string.IsNullOrWhiteSpace(at.topic))
                                        at.topic += "," + value_name;
                                    else
                                        at.topic = value_name;
                                }
                            }
                            else if (type_name == "Property")
                            {
                                if (!propertyDic.ContainsKey(value_name))
                                {
                                    propertyDic.Add(value_name, 0);
                                    if (!string.IsNullOrWhiteSpace(at.property))
                                        at.property += "," + value_name;
                                    else
                                        at.property = value_name;
                                }
                            }
                            var country_name = GetDBString("country_name", reader);
                            if (!string.IsNullOrWhiteSpace(country_name) && LocationTypeID == 2)
                            {
                                if (!countryDic.ContainsKey(country_name))
                                {
                                    countryDic.Add(country_name, 0);
                                    if (!string.IsNullOrWhiteSpace(at.country))
                                        at.country += "," + country_name;
                                    else
                                        at.country = country_name;
                                }
                            }
                            var city_name = GetDBString("city_name", reader);
                            if (!string.IsNullOrWhiteSpace(city_name) && LocationTypeID == 3)
                            {
                                if (!cityDic.ContainsKey(city_name))
                                {
                                    cityDic.Add(city_name, 0);
                                    if (!string.IsNullOrWhiteSpace(at.city))
                                        at.city += "|" + city_name;
                                    else
                                        at.city = city_name;
                                }
                            }
                            var company_name = GetDBString("company_name", reader);
                            if (!string.IsNullOrWhiteSpace(company_name))
                            {
                                if (!companyDic.ContainsKey(company_name))
                                {
                                    companyDic.Add(company_name, 0);
                                    if (!string.IsNullOrWhiteSpace(at.company))
                                        at.company += "|" + company_name;
                                    else
                                        at.company = company_name;
                                }
                            }
                        }
                        if (at.article_id != -1)
                            articles.Add(at);
                        reader.Close();
                        reader = null;
                    }
                    this.CloseConnection();
                }
                //    file.WriteLine("exit from getArticles, result is " + articles.Count + " articles");
                //    file.WriteLine(DateTime.Now.ToString());
                //}
            }
            catch (Exception ex)
            {
                articles[0].title = ex.Message;
                articles.Clear();
            }
            return articles;
        }

        public Article1 getArticle(int id)
        {
            Article1 at = null;
            try
            {
                if (this.OpenConnection() == true)
                {
                    //create command and assign the query and connection from the constructor
                    string query = "call usp_get_article(" + id.ToString() + ");";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        MySqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            string date = GetDBTime("ArticleDate", reader).ToString("dd-MM-yyyy");
                            var url = GetDBString("ArticleUrl", reader);
                            var content = GetDBString("Content_Short", reader);
                            var title = GetDBString("Title", reader);

                            at = new Article1
                            {
                                content = content,
                                date = date,
                                url = url,
                                title = title
                            };
                        }
                    }
                    this.CloseConnection();
                }
            }
            catch (Exception ex)
            {
            }
            return at;
        }

        public DashboardData getDashboardData(string country, int period)
        {
            DashboardData dashboardData = new DashboardData();
            try
            {
                if (this.OpenConnection() == true)
                {
                    string query = "select Date, Value, NewResidentialConstruction, NationalHomePriceIndex from tbl_market_sentiment " +
                    "left join tbl_countries on tbl_market_sentiment.CountryId = tbl_countries.ID " +
                    "Where tbl_countries.Name=@countryName AND tbl_market_sentiment.Period=@period " +
                    "ORDER BY tbl_market_sentiment.Date DESC";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.Add("@countryName", MySqlDbType.VarChar).Value = country;
                        command.Parameters.Add("@period", MySqlDbType.Int16).Value = period;
                        MySqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            dashboardData.market_sentiment.Add(new Market_sentiment
                            {
                                date = GetDBTime("Date", reader).ToString("dd-MM-yyyy"),
                                sentiment = GetDBFloat("Value", reader),
                                newResidentialConstruction = GetDBFloat("NewResidentialConstruction", reader),
                                nationalHomePriceIndex = GetDBFloat("NationalHomePriceIndex", reader)
                            });
                        }
                    }
                    this.CloseConnection();
                }
                dashboardData.highlights.Add(new Highlights
                {
                    date = new DateTime(2015, 8, 18).ToString("dd-MM-yyyy"),
                    title = "Q2 2015 U.S. Office MarketView",
                    topic = "CBRE",
                    url = "http://www.cbre.us/research/2015-US-Reports/Pages/Q2-2015-US-Office-MarketView.aspx"
                });
                dashboardData.highlights.Add(new Highlights
                {
                    date = new DateTime(2015, 8, 11).ToString("dd-MM-yyyy"),
                    title = "Q2 2015 U.S. Industrial MarketView",
                    topic = "CBRE",
                    url = "http://www.cbre.us/research/2015-US-Reports/Pages/Q2-2015-US-Industrial-MarketView.aspx"
                });
                dashboardData.highlights.Add(new Highlights
                {
                    date = new DateTime(2015, 7, 1).ToString("dd-MM-yyyy"),
                    title = "Resurgence in Midwest Secondary Markets: Implications for Occupiers",
                    topic = "CBRE",
                    url = "http://www.cbre.us/research/2015-US-Reports/Pages/Resurgence-in-Midwest-Secondary-Markets-Implications-for-Occupiers.aspx"
                });

                dashboardData.highlights.Add(new Highlights
                {
                    date = new DateTime(2015, 8, 17).ToString("dd-MM-yyyy"),
                    title = "2nd Qtr 2015 Industrial Market Report",
                    topic = "Colliers",
                    url = "http://www.colliers.com/-/media/files/marketresearch/unitedstates/markets/jacksonville/quarterly_reports/2015_quarterly_reports/industrial/q2-2015_industrial_nefl.pdf?la=en-US"
                });
                dashboardData.highlights.Add(new Highlights
                {
                    date = new DateTime(2015, 8, 17).ToString("dd-MM-yyyy"),
                    title = "Mid Year 2015 Multi-Family Market Report",
                    topic = "Colliers",
                    url = "http://www.colliers.com/-/media/files/marketresearch/unitedstates/markets/jacksonville/quarterly_reports/2015_quarterly_reports/multifamily/mid-year%202015_multifamily_nefl.pdf?la=en-US"
                });
                dashboardData.highlights.Add(new Highlights
                {
                    date = new DateTime(2015, 8, 17).ToString("dd-MM-yyyy"),
                    title = "Apartment Mid-Year 2015",
                    topic = "Colliers",
                    url = "http://www.colliers.com/-/media/files/united%20states/markets/minnesota/market%20reports/q2%202015/apartment-q2-2015.pdf?la=en-US"
                });

                dashboardData.highlights.Add(new Highlights
                {
                    date = new DateTime(2015, 8, 17).ToString("dd-MM-yyyy"),
                    title = "Revisions Support Septemeber Rate Increase",
                    topic = "Cushman & Wakefield",
                    url = "http://www.cushmanwakefield.com/~/media/reports/unitedstates/2014/Revisions%20Support%20September%20Rate%20Increase%20weekly%20update%208-17-15.pdf"
                });
                dashboardData.highlights.Add(new Highlights
                {
                    date = new DateTime(2015, 8, 10).ToString("dd-MM-yyyy"),
                    title = "The Commercial Real Estate environment keeps getting better",
                    topic = "Cushman & Wakefield",
                    url = "http://www.cushmanwakefield.com/~/media/reports/unitedstates/2014/Environment%20Keeps%20Getting%20Better-Weekly%20Update%208-10-15.pdf"
                });
                dashboardData.highlights.Add(new Highlights
                {
                    date = new DateTime(2015, 8, 3).ToString("dd-MM-yyyy"),
                    title = "Fed Time is Coming",
                    topic = "Cushman & Wakefield",
                    url = "http://www.cushmanwakefield.com/~/media/reports/unitedstates/2014/Fed_Time_is_Coming_Weekly_Report_8_3_15.pdf"
                });

                //if (this.OpenConnection() == true)
                //{
                //    string query = "select topic, title, date, url from tbl_highlights " +
                //    "left join tbl_countries on tbl_highlights.CountryId = tbl_countries.ID " +
                //    "Where tbl_countries.Name=@countryName " +
                //    "ORDER BY tbl_highlights.date DESC";
                //    using (var command = new MySqlCommand(query, connection))
                //    {
                //        command.Parameters.Add("@countryName", MySqlDbType.VarChar).Value = country;
                //        MySqlDataReader reader = command.ExecuteReader();
                //        while (reader.Read())
                //        {
                //            dashboardData.highlights.Add(new Highlights
                //            {
                //                date = GetDBTime("date", reader).ToString("dd-MM-yyyy"),
                //                title = GetDBString("title", reader),
                //                topic = GetDBString("topic", reader),
                //                url = GetDBString("url", reader)
                //            });
                //        }
                //    }
                //    this.CloseConnection();
                //}
                if (this.OpenConnection() == true)
                {
                    DateTime startDate = DateTime.Now.AddMonths(-period);
                     string query = "select Property, Date, BuySell, Construction, Rent from tbl_comparision_data " +
                    "left join tbl_countries on tbl_comparision_data.CountryId = tbl_countries.ID " +
                    "Where tbl_countries.Name=@countryName AND Date>@startDate " +
                    "ORDER BY tbl_comparision_data.date DESC";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.Add("@countryName", MySqlDbType.VarChar).Value = country;
                        command.Parameters.Add("@startDate", MySqlDbType.DateTime).Value = startDate;
                        MySqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            dashboardData.comparison_data.Add(new Comparison_data
                            {
                                date = GetDBTime("date", reader).ToString("dd-MM-yyyy"),
                                property = GetDBString("Property", reader),
                                buy_sell = GetDBInt("BuySell", reader),
                                construction = GetDBInt("Construction", reader),
                                rent = GetDBInt("Rent", reader),
                            });
                        }
                    }
                    this.CloseConnection();
                }
                if (this.OpenConnection() == true)
                {
                    DateTime startDate = DateTime.Now.AddMonths(-period);
                    string query = "select Date, UnemploymentRate, Inflation, HousePriceIndexChange,RateForLodging,RateForOffice,RateForCommercial,RateForHealthcare,RateForLeasure,RateForNonResidential,RateForResidential from tbl_macro " +
                    "left join tbl_countries on tbl_macro.CountryId = tbl_countries.ID " +
                    "Where tbl_countries.Name=@countryName AND Date>@startDate " +
                    "ORDER BY tbl_macro.date DESC";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.Add("@countryName", MySqlDbType.VarChar).Value = country;
                        command.Parameters.Add("@startDate", MySqlDbType.DateTime).Value = startDate;
                        MySqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            dashboardData.macro.Add(new Macro
                            {
                                date = GetDBTime("date", reader).ToString("dd-MM-yyyy"),
                                UnemploymentRate = GetDBFloat("UnemploymentRate", reader),
                                Inflation = GetDBFloat("Inflation", reader),
                                HousePriceIndexChange = GetDBFloat("HousePriceIndexChange", reader),
                                RateForLodging = GetDBFloat("RateForLodging", reader),
                                RateForOffice = GetDBFloat("RateForOffice", reader),
                                RateForCommercial = GetDBFloat("RateForCommercial", reader),
                                RateForHealthcare = GetDBFloat("RateForHealthcare", reader),
                                RateForLeasure = GetDBFloat("RateForLeasure", reader),
                                RateForNonResidential = GetDBFloat("RateForNonResidential", reader),
                                RateForResidential = GetDBFloat("RateForResidential", reader)
                            });
                        }
                    }
                    this.CloseConnection();
                }
                if (this.OpenConnection() == true)
                {
                    string query = "select tbl_general_information.Name, Value from tbl_general_information " +
                    "left join tbl_countries on tbl_general_information.CountryId = tbl_countries.ID " +
                    "Where tbl_countries.Name=@countryName";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.Add("@countryName", MySqlDbType.VarChar).Value = country;
                        MySqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            dashboardData.generalInformation.Add(new GeneralInformation
                            {
                                Name = GetDBString("Name", reader),
                                Value = GetDBString("Value", reader)
                            });
                        }
                    }
                    this.CloseConnection();
                }
            }
            catch (Exception ex)
            {
                string exc = ex.Message;
            }
            return dashboardData;
    }

        public bool login(string name, string password)
        {
            bool res = false;
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                string query = "select * from tbl_users where Username='" + name + "' AND Password='" + password + "';";
                using (var command = new MySqlCommand(query, connection))
                {
                    MySqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        res = true;
                    }
                }
                this.CloseConnection();
            }
            return res;
        }

        private DateTime dateFromString(string dt)
        {
            DateTime dateResult;
            if (DateTime.TryParse(dt, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.None, out dateResult))
                return dateResult;
            if (DateTime.TryParse(dt, CultureInfo.CreateSpecificCulture("de-DE"), DateTimeStyles.None, out dateResult))
                return dateResult;
            return DateTime.MinValue;
        }

        private string GetDBString(string SqlFieldName, MySqlDataReader Reader)
        {
            return Reader[SqlFieldName].Equals(DBNull.Value) ? String.Empty : Reader.GetString(SqlFieldName);
        }

        private DateTime GetDBTime(string SqlFieldName, MySqlDataReader Reader)
        {
            return Reader[SqlFieldName].Equals(DBNull.Value) ? DateTime.MinValue : Reader.GetDateTime(SqlFieldName);
        }

        private int GetDBInt(string SqlFieldName, MySqlDataReader Reader)
        {
            return Reader[SqlFieldName].Equals(DBNull.Value) ? -1 : Reader.GetInt32(SqlFieldName);
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
                return false;
            }
        }
    }
}
