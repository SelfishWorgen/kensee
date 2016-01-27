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
        public List<string[]> selectArticlesOld(string type) //old only, used in update
        {
            List<string[]> result = new List<string[]>();
            if (OpenConnection())
            {
                string query = "select * from tb_news where type='" + type + "'";
                using (var command = new MySqlCommand(query, connection))
                {
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string[] row = new string[3];
                        row[0] = GetDBString("news_url", reader);
                        row[1] = GetDBString("sentiment", reader);
                        row[2] = GetDBString("news_content", reader);
                        result.Add(row);
                    }
                }

                this.CloseConnection();
            }

            return result;
        }

        public void UpdateArticleOld(string url, string type, string sentiment, string reEvent, string country)
        {
            string query = "";
            if (reEvent != "")
            {
                query = @"UPDATE tb_news SET event='" + reEvent + "' ";
                if (sentiment != "")
                    query += ", sentiment= '" + sentiment + "' ";
                if (country != "")
                    query += ", country= '" + country + "' ";
            }
            else if (sentiment != "")
            {
                query = @"UPDATE tb_news SET sentiment='" + sentiment + "' ";
            }
            query += "WHERE news_url='" + url.Trim() + "' AND type='" + type + "'";

            if (this.OpenConnection() == true)
            {
                using (var command = new MySqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
                this.CloseConnection();
            }
        }

        public List<int> getAricleIds()
        {
            List<int> res = new List<int>();
            if (OpenConnection())
            {
                string query = "select Id from tbl_articles";
                using (var command = new MySqlCommand(query, connection))
                {
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(GetDBInt("Id", reader));
                    }
                }
                CloseConnection();
            }
            return res;
        }

        public void getArticleDetails(int articleId,
            out string content,
            out int sourceTypeId,
            out int sourceId,
            out List<UpdateArticleTags> articleTags,
            out List<UpdateRecompanies> articleCompanies,
            out List<UpdateLocations> articleLocations)
        {
            content = "";
            sourceTypeId = 0;
            sourceId = 0;
            articleTags = new List<UpdateArticleTags>();
            articleCompanies = new List<UpdateRecompanies>();
            articleLocations = new List<UpdateLocations>();
            string query;
            if (OpenConnection())
            {
                query = "select content, sourcetypeId, sourceid from tbl_articles where id = '" + articleId.ToString() + "'";
                using (var command = new MySqlCommand(query, connection))
                {
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        content = GetDBString("content", reader);
                        sourceTypeId = GetDBInt("sourcetypeId", reader);
                        sourceId = GetDBInt("sourceId", reader);
                    }
                    reader.Close();
                }

                query = "select id, articletagtypevalue from tbl_articletags where articleid = '" + articleId.ToString() + "'";
                using (var command = new MySqlCommand(query, connection))
                {
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int id = GetDBInt("id", reader);
                        int articletagtypevalue = GetDBInt("articletagtypevalue", reader);
                        articleTags.Add(new UpdateArticleTags { Id = id, ArticleTagTypeValue = articletagtypevalue, isUsed = false });
                    }
                    reader.Close();
                }
                query = "select id, recompanyid from tbl_articlecompanies where articleid = '" + articleId.ToString() + "'";
                using (var command = new MySqlCommand(query, connection))
                {
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int id = GetDBInt("id", reader);
                        int recompanyid = GetDBInt("recompanyid", reader);
                        articleCompanies.Add(new UpdateRecompanies { Id = id, ReCompanyId = recompanyid, isUsed = false });
                    }
                    reader.Close();
                }
                query = "select id, locationtypeid, locationid from tbl_articlelocations where articleid = '" + articleId.ToString() + "'";
                using (var command = new MySqlCommand(query, connection))
                {
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int id = GetDBInt("id", reader);
                        int locationtypeid = GetDBInt("locationtypeid", reader);
                        int locationid = GetDBInt("locationid", reader);
                        articleLocations.Add(new UpdateLocations { Id = id, LocationTypeId = locationtypeid, LocationId = locationid, isUsed = false });
                    }
                    reader.Close();
                }
                CloseConnection();
            }
        }

        public int getCountryIdByCompany(int sourceId)
        {
            int countryId = 0;
            if (OpenConnection())
            {
                string query = "select countryId from tbl_recompanies where id = '" + sourceId.ToString() + "'";
                using (var command = new MySqlCommand(query, connection))
                {
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        countryId = GetDBInt("countryId", reader);
                    }
                }
                CloseConnection();
            }
            return countryId;
        }

        public void updateArticle(int articleId, article a,
            List<UpdateArticleTags> articleTags, 
            List<UpdateRecompanies> articleCompanies,
            List<UpdateLocations> articleLocations)
        {
            // update in article sentiment (sentimentValue in article), sentimentTypeId
            string query = "";
            if (OpenConnection())
            {
                query = "update tbl_articles set sentiment=@sentiment, sentimentTypeId=@sentimentTypeId, updateddate=NOW() " +
                    "where id = @articleid";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.Add("@articleid", MySqlDbType.Int32).Value = articleId;
                    command.Parameters.Add("@sentiment", MySqlDbType.Float).Value = a.sentimentValue;
                    command.Parameters.Add("@sentimenttypeid", MySqlDbType.Int32).Value = a.sentimentTypeId;
                    command.ExecuteNonQuery();
                }
                foreach (UpdateArticleTags at in articleTags)
                {
                    if (!at.isUsed)
                    {
                        query = "delete from tbl_articletags where id = '" + at.Id.ToString() + "'";
                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    else if (at.Id == 0)
                    {
                        query = "INSERT INTO tbl_articletags (articleid, articletagtypevalue) " +
                                "VALUES(@articleid, @articletagtypevalue)";
                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.Add("@articleid", MySqlDbType.Int32).Value = articleId;
                            command.Parameters.Add("@articletagtypevalue", MySqlDbType.Int32).Value = at.ArticleTagTypeValue;
                            command.ExecuteNonQuery();
                        }
                    }
                }
                foreach (UpdateRecompanies ac in articleCompanies)
                {
                    if (!ac.isUsed)
                    {
                        query = "delete from tbl_articlecompanies where id = '" + ac.Id.ToString() + "'";
                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    else if (ac.Id == 0)
                    {
                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.Add("@articleid", MySqlDbType.Int32).Value = articleId;
                            command.Parameters.Add("@recompanyid", MySqlDbType.Int32).Value = ac.ReCompanyId;
                            command.ExecuteNonQuery();
                        }
                    }
                }
                foreach (UpdateLocations al in articleLocations)
                {
                    if (!al.isUsed)
                    {
                        query = "delete from tbl_articlelocations where id = '" + al.Id.ToString() + "'";
                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    else if (al.Id == 0)
                    {
                        query = "INSERT INTO tbl_articlelocations (locationtypeid, locationid, articleid) " +
                                "VALUES(@locationtypeid, @locationid, @articleid)";
                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.Add("@articleid", MySqlDbType.Int32).Value = articleId;
                            command.Parameters.Add("@locationtypeid", MySqlDbType.Int32).Value = al.LocationTypeId;
                            command.Parameters.Add("@locationid", MySqlDbType.Int32).Value = al.LocationId;
                            command.ExecuteNonQuery();
                        }
                    }
                }
                CloseConnection();
            }
        }

    }

}

