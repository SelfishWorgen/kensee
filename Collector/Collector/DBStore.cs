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
        public void storeArticleToDB(article item)
        {

            if (OpenConnection())
            {
                long articleId = 0;
                string query = "INSERT INTO tbl_articles (title, content, sourceid, sourcetypeId, sentiment, sentimenttypeid, articledate, articleurl, content_short, snippet, creationdate, updateddate, qa_status, qa_comment) " +
                    "VALUES(@title, @content, @sourceid, @sourcetypeId, @sentiment, @sentimenttypeid, @articledate, @articleurl, @content_short, @snippet, NOW(), NOW(), 0, '')";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.Add("@title", MySqlDbType.VarChar).Value = item.title;
                    command.Parameters.Add("@content", MySqlDbType.Text).Value = item.content;
                    command.Parameters.Add("@sourceid", MySqlDbType.Int32).Value = item.sourceId;
                    command.Parameters.Add("@sourcetypeId", MySqlDbType.Int32).Value = item.sourceTypeId;
                    command.Parameters.Add("@sentiment", MySqlDbType.Float).Value = item.sentimentValue;
                    command.Parameters.Add("@sentimenttypeid", MySqlDbType.Int32).Value = item.sentimentTypeId;
                    command.Parameters.Add("@articledate", MySqlDbType.DateTime).Value = item.dt;
                    command.Parameters.Add("@articleurl", MySqlDbType.VarChar).Value = item.href;
                    command.Parameters.Add("@content_short", MySqlDbType.Text).Value = item.content_short;
                    command.Parameters.Add("@snippet", MySqlDbType.Text).Value = item.snippet;
                    command.ExecuteNonQuery();

                    articleId = command.LastInsertedId;
                }
                foreach (int eventId in item.eventIds)
                {
                    query = "INSERT INTO tbl_articletags (articleid, articletagtypevalue) " +
                            "VALUES(@articleid, @articletagtypevalue)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.Add("@articleid", MySqlDbType.Int32).Value = articleId;
                        command.Parameters.Add("@articletagtypevalue", MySqlDbType.Int32).Value = eventId;
                        command.ExecuteNonQuery();
                    }
                }
                foreach (int propertyId in item.propertyIds)
                {
                    query = "INSERT INTO tbl_articletags (articleid, articletagtypevalue) " +
                            "VALUES(@articleid, @articletagtypevalue)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.Add("@articleid", MySqlDbType.Int32).Value = articleId;
                        command.Parameters.Add("@articletagtypevalue", MySqlDbType.Int32).Value = propertyId;
                        command.ExecuteNonQuery();
                    }
                }
                foreach (int companyId in item.companyIds)
                {
                    query = "INSERT INTO tbl_articlecompanies (articleid, recompanyid) " +
                            "VALUES(@articleid, @recompanyid)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.Add("@articleid", MySqlDbType.Int32).Value = articleId;
                        command.Parameters.Add("@recompanyid", MySqlDbType.Int32).Value = companyId;
                        command.ExecuteNonQuery();
                    }
                }
                foreach (GeoLocation loc in item.locationIds)
                {
                    query = "INSERT INTO tbl_articlelocations (locationtypeid, locationid, articleid) " +
                            "VALUES(@locationtypeid, @locationid, @articleid)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.Add("@articleid", MySqlDbType.Int32).Value = articleId;
                        command.Parameters.Add("@locationtypeid", MySqlDbType.Int32).Value = loc.locationType;
                        command.Parameters.Add("@locationid", MySqlDbType.Int32).Value = loc.locationId;
                        command.ExecuteNonQuery();
                    }
                }
                foreach (HierarchicalEvent he in item.hierarchicalEvents)
                {
                    query = "INSERT INTO tbl_eventdetails (articleid, tagtypevalue, propertyid, countryid, cityid, area, price, date, companies) " +
                            "VALUES(@articleid, @tagtypevalue, @propertyid, @countryid, @cityid, @area, @price, @date, @companies)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.Add("@articleid", MySqlDbType.Int32).Value = articleId;
                        command.Parameters.Add("@tagtypevalue", MySqlDbType.Int32).Value = he.eventId;
                        command.Parameters.Add("@propertyid", MySqlDbType.Int32).Value = he.propertyId;
                        command.Parameters.Add("@countryid", MySqlDbType.Int32).Value = he.location.countryId;
                        command.Parameters.Add("@cityid", MySqlDbType.Int32).Value = he.location.cityId < 0 ? 0 : he.location.cityId;
                        command.Parameters.Add("@area", MySqlDbType.String).Value = he.areaStr;
                        command.Parameters.Add("@price", MySqlDbType.String).Value = he.priceStr;
                        command.Parameters.Add("@date", MySqlDbType.String).Value = he.dateStr;
                        command.Parameters.Add("@companies", MySqlDbType.String).Value = he.companyNamesStr;
                        command.ExecuteNonQuery();
                    }
                }

                CloseConnection();
            }
        }

        public void deleteArticle(int id)
        {
            string query;

            if (OpenConnection())
            {
                query = "delete from tbl_articles where id = '" + id.ToString() + "'";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
                query = "delete from tbl_articlelocations where articleid = '" + id.ToString() + "'";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
                query = "delete from tbl_articletags where articleid = '" + id.ToString() + "'";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
                query = "delete from tbl_articlecompanies where articleid = '" + id.ToString() + "'";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
                CloseConnection();
            }
        }


    }
}
