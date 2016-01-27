using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;


namespace Collector
{
    partial class DBConnect
    {
        public List<companyTicker> getTickersOld(string nm)
        {
            List<companyTicker> res = new List<companyTicker>();
            string fieldName = "";
            switch (nm)
            {
                case "bloomberg":
                    fieldName = "ticker_bloomberg";
                    break;
                case "reutars":
                    fieldName = "ticker_reuters";
                    break;
                case "yahoo":
                    fieldName = "ticker_yahoo";
                    break;
            }
            if (fieldName == "")
                return res;
            if (OpenConnection())
            {
                string query = "select * from tb_site where del_flag='0' and " + fieldName + " != ''";
                using (var command = new MySqlCommand(query, connection))
                {
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(new companyTicker
                        {
                            companyName = GetDBString("company_name", reader),
                            country = GetDBString("country", reader),
                            ticker = GetDBString(fieldName, reader),
                            industry = GetDBString("industry", reader),
                            countryId = 0,
                            id = 0,
                        });

                    }
                }
            }
            CloseConnection();
            return res;
        }

        public List<companyTicker> getTickers(string nm)
        {
            List<companyTicker> res = new List<companyTicker>();
            string fieldName = "";
            switch (nm)
            {
                case "bloomberg":
                    fieldName = "BloombergSymbol";
                    break;
                case "reuters":
                    fieldName = "ReutersSymbol";
                    break;
                case "yahoo":
                    fieldName = "YahooSymbol";
                    break;
            }
            if (fieldName == "")
                return res;

            if (OpenConnection())
            {
                string query = "select * from tbl_recompanies where " + fieldName + " != '' and isactive = '1'";
                using (var command = new MySqlCommand(query, connection))
                {
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(new companyTicker
                        {
                            id = GetDBInt("Id", reader),
                            companyName = GetDBString("Name", reader),
                            country = "",
                            ticker = GetDBString(fieldName, reader),
                            industry = "",
                            countryId = GetDBInt("CountryId", reader)
                        });

                    }
                }
            }
            CloseConnection();
            return res;
        }

        public int searchSourceIdInRecompanies(string nm, out int isActive, out int countryId)
        {
            int res = 0;
            isActive = 0;
            countryId = 0;
            if (OpenConnection())
            {
                string query = "select id, isActive, countryId from tbl_recompanies where name = '" + nm + "'";
                using (var command = new MySqlCommand(query, connection))
                {
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res = GetDBInt("id", reader);
                        isActive = GetDBInt("isActive", reader);
                        countryId = GetDBInt("countryId", reader);
                        break;
                    }
                }
            }
            CloseConnection();
            return res;

        }
    }
}
