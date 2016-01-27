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
                        string urlRule = GetDBString("UrlRule", reader).Trim();
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

        public void endScraping(int id)
        {
            string query = @"UPDATE tbl_newswebsites SET CreationDate='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            query += "WHERE id='" + id.ToString() + "'";

            if (this.OpenConnection() == true)
            {
                using (var command = new MySqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
                this.CloseConnection();
            }
        }

    }
}
