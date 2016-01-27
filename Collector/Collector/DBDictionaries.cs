using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using ReEvents;

namespace Collector
{
    partial class DBConnect
    {
        public List<ReEventsLocType> getLocationTypes()
        {
            List<ReEventsLocType> lst = new List<ReEventsLocType>();
            if (!OpenConnection())
                return lst;
            string query = "select * from tbl_locationTypes";
            using (var command = new MySqlCommand(query, connection))
            {
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ReEventsLocType rec = new ReEventsLocType();
                    rec.locTypeId = GetDBInt("ID", reader);
                    rec.locTypeName = GetDBString("Name", reader);
                    if (rec.locTypeName != "" && rec.locTypeId != -1)
                        lst.Add(rec);
                }
            }
            CloseConnection();
            return lst;
        }
        
        public List<ReEventsCountryName> getCountryNames()
        {
            List<ReEventsCountryName> lst = new List<ReEventsCountryName>();
            if (!OpenConnection())
                return lst;
            string query = "select * from tbl_countryDictionary";
            using (var command = new MySqlCommand(query, connection))
            {
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ReEventsCountryName rec = new ReEventsCountryName();
                    rec.countryId = GetDBInt("CountryID", reader);
                    rec.countryName = GetDBString("Name", reader);
                    if (rec.countryName != "" && rec.countryId != -1)
                        lst.Add(rec);
                }
            }
            CloseConnection();
            return lst;
        }

        public List<ReEventsStateName> getStateNames()
        {
            List<ReEventsStateName> lst = new List<ReEventsStateName>();

            if (!OpenConnection())
                return lst;
            string query = "select * from tbl_stateDictionary";
            using (var command = new MySqlCommand(query, connection))
            {
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ReEventsStateName rec = new ReEventsStateName();
                    rec.stateId = GetDBInt("StateID", reader);
                    rec.stateName = GetDBString("Name", reader); 
                    if (rec.stateName != "" && rec.stateId != -1)
                        lst.Add(rec);
                }
            }
            CloseConnection();
            return lst;
        }

        public List<ReEventsCityName> getCityNames()
        {
            List<ReEventsCityName> lst = new List<ReEventsCityName>();
            if (!OpenConnection())
                return lst;
            string query = "select * from tbl_cityDictionary";
            using (var command = new MySqlCommand(query, connection))
            {
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ReEventsCityName rec = new ReEventsCityName();
                    rec.cityId = GetDBInt("CityID", reader);
                    rec.cityName = GetDBString("Name", reader);
                    if (rec.cityName != "" && rec.cityId != -1)
                        lst.Add(rec);
                }
            }
            CloseConnection();
            return lst;
        }

        public List<ReEventsCity> getCityCountryIds()
        {
            List<ReEventsCity> lst = new List<ReEventsCity>();
            if (!OpenConnection())
                return lst;
            string query = "select * from tbl_Cities";
            using (var command = new MySqlCommand(query, connection))
            {
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ReEventsCity rec = new ReEventsCity();
                    rec.cityId = GetDBInt("ID", reader);
                    rec.countryId = GetDBInt("CountryID", reader);
                    rec.stateId = GetDBInt("StateID", reader);
                    rec.uniqCityName = GetDBString("Name", reader);
                    if (rec.countryId != -1 && rec.cityId != -1)
                        lst.Add(rec);
                }
            }
            CloseConnection();
            return lst;
        }

        public List<ReEventsState> getStateCountryIds()
        {
            List<ReEventsState> lst = new List<ReEventsState>();
            if (!OpenConnection())
                return lst;
            string query = "select * from tbl_States"; 
            using (var command = new MySqlCommand(query, connection))
            {
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ReEventsState rec = new ReEventsState();
                    rec.stateId = GetDBInt("ID", reader);
                    rec.countryId = GetDBInt("CountryID", reader);
                    rec.uniqStateName = GetDBString("Name", reader);
                    if (rec.countryId != -1 && rec.stateId != -1)
                        lst.Add(rec);
                }
            }
            CloseConnection();
            return lst;
        }

        public List<ReEventsCountry> getCountryRegionIds()
        {
            List<ReEventsCountry> lst = new List<ReEventsCountry>();
            if (!OpenConnection())
                return lst;
            string query = "select * from tbl_Countries";
            using (var command = new MySqlCommand(query, connection))
            {
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ReEventsCountry rec = new ReEventsCountry();
                    rec.regionId = GetDBInt("RegionID", reader);
                    rec.countryId = GetDBInt("ID", reader);
                    rec.uniqCountryName = GetDBString("Name", reader);
                    if (rec.countryId != -1 && rec.regionId != -1 && rec.uniqCountryName != "")
                        lst.Add(rec);
                }
            }
            CloseConnection();
            return lst;
        }

        public List<ReEventsRegionName> getRegionNames()
        {
            List<ReEventsRegionName> lst = new List<ReEventsRegionName>();
            if (!OpenConnection())
                return lst;
            string query = "select * from tbl_regions";
            using (var command = new MySqlCommand(query, connection))
            {
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ReEventsRegionName rec = new ReEventsRegionName();
                    rec.regionId = GetDBInt("Id", reader);
                    rec.regionName = GetDBString("Name", reader);
                    if (rec.regionId != -1 && rec.regionName != "")
                        lst.Add(rec);
                }
            }
            CloseConnection();
            return lst;
        }

        public List<ReEventsPropertyKeyword> getPropertyKeywords()
        {
            List<ReEventsPropertyKeyword> lst = new List<ReEventsPropertyKeyword>();
            if (!OpenConnection())
                return lst;
            string query = "select * from tbl_articletagtypevaluedict where ArticleTagTypeValue in " +
                "(select id from tbl_articletagtypevalues where articlesubtagtypeid != '3')";
            using (var command = new MySqlCommand(query, connection))
            {
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ReEventsPropertyKeyword rec = new ReEventsPropertyKeyword();
                    rec.propertyId = GetDBInt("ArticleTagTypeValue", reader);
                    rec.propertyName = GetDBString("Name", reader);
                    if (rec.propertyId != -1 && rec.propertyName != "")
                        lst.Add(rec);
                }
            }
            CloseConnection();
            return lst;
        }

        public List<ReEventsPropertyName> getPropertyNames()
        {
            List<ReEventsPropertyName> lst = new List<ReEventsPropertyName>();
            if (!OpenConnection())
                return lst;
            string query = "select * from tbl_articletagtypevalues";
            using (var command = new MySqlCommand(query, connection))
            {
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ReEventsPropertyName rec = new ReEventsPropertyName();
                    rec.propertyId = GetDBInt("Id", reader);
                    rec.propertyName = GetDBString("Name", reader);
                    if (rec.propertyId != -1 && rec.propertyName != "")
                        lst.Add(rec);
                }
            }
            CloseConnection();
            return lst;
        }

        public List<ReEventsCompanyName> getCompanies()
        {
            List<ReEventsCompanyName> lst = new List<ReEventsCompanyName>();
            if (!OpenConnection())
                return lst;
            string query = "select id, name, countryId from tbl_recompanies";
            using (var command = new MySqlCommand(query, connection))
            {
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ReEventsCompanyName rec = new ReEventsCompanyName();
                    rec.companyId = GetDBInt("Id", reader);
                    rec.companyName = GetDBString("Name", reader);
                    rec.countryId = GetDBInt("countryId", reader);
                    if (rec.companyId != -1 && rec.companyName != "")
                        lst.Add(rec);
                }
            }
            CloseConnection();
            return lst;
        }

        public List<ReEventsTagTypeValues> getEventTagTypeValues()
        {
            List<ReEventsTagTypeValues> lst = new List<ReEventsTagTypeValues>();
            if (!OpenConnection())
                return lst;
            string query = "select id, name from tbl_articletagtypevalues";
            using (var command = new MySqlCommand(query, connection))
            {
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ReEventsTagTypeValues rec = new ReEventsTagTypeValues();
                    rec.eventId = GetDBInt("Id", reader);
                    rec.eventName = GetDBString("Name", reader);
                    if (rec.eventId != -1 && rec.eventName != "")
                        lst.Add(rec);
                }
            }
            CloseConnection();
            return lst;
        }

        public void getSourceTypes(out int reCompanyType, out int newsWebSite)
        {
            reCompanyType = 2;
            newsWebSite = 1;
            if (!OpenConnection())
                return;
            string query = "select * from tbl_sourcetypes";
            using (var command = new MySqlCommand(query, connection))
            {
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string nm = GetDBString("Name", reader);
                    if (nm == "News Website")
                        newsWebSite = GetDBInt("Id", reader);
                    if (nm == "RE Company")
                        reCompanyType = GetDBInt("Id", reader);
                }
            }
            CloseConnection();
        }

        public void getSentimentTypes(out int positiveType, out int neutralType, out int negativeType)
        {
            positiveType = 1;
            neutralType = 2;
            negativeType = 3;
            if (!OpenConnection())
                return;
            string query = "select * from tbl_sentimenttypes";
            using (var command = new MySqlCommand(query, connection))
            {
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string nm = GetDBString("Name", reader);
                    if (nm == "Positive")
                        positiveType = GetDBInt("Id", reader);
                    if (nm == "Neutral")
                        neutralType = GetDBInt("Id", reader);
                    if (nm == "Negative")
                        negativeType = GetDBInt("Id", reader);
                }
            }
            CloseConnection();
        }

        public List<ReEventsTagTypeValues> getEventNames()
        {
            List<ReEventsTagTypeValues> lst = new List<ReEventsTagTypeValues>();
            if (!OpenConnection())
                return lst;
            string query = "select id, name from tbl_articletagtypevalues where articleSubTagTypeId = 3 and id != 10";
            using (var command = new MySqlCommand(query, connection))
            {
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ReEventsTagTypeValues rec = new ReEventsTagTypeValues();
                    rec.eventId = GetDBInt("Id", reader);
                    rec.eventName = GetDBString("Name", reader);
                    if (rec.eventId != -1 && rec.eventName != "")
                        lst.Add(rec);
                }
            }
            CloseConnection();
            return lst;
        }

        public List<int> getNotShowTags()
        {
            List<int> lst = new List<int>();
            if (!OpenConnection())
                return lst;
            string query = "select id from tbl_articletagtypevalues where notshow = 1";
            using (var command = new MySqlCommand(query, connection))
            {
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int id = GetDBInt("Id", reader);
                    if (id != -1)
                        lst.Add(id);
                }
            }
            CloseConnection();
            return lst;
        }

    }
}
