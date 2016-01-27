using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HtmlAgilityPack;
using System.Threading;
using MySql.Data.MySqlClient;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Globalization;
using System.Text.RegularExpressions;

using ReEvents;

namespace Collector
{
    public partial class Form1 : Form
    {

        public void UpdateDb()  
        {
            DBConnect db_obj = new DBConnect("kensee");
            List<int> articleIds = db_obj.getAricleIds();

            int cnt = articleIds.Count;
            int i = 0;

            string strToLog = getLogTitle("Update News");
            reEventsParser.writeToLog(strToLog);
            reEventsParser.writeToLog(cnt.ToString() + " articles in database");

            this.Invoke((MethodInvoker)delegate
            {
                progressBar1.Maximum = cnt;
                progressBar1.Value = 0;
            });

            foreach (int aId in articleIds)
            {
                string content;
                int sourceTypeId;
                int sourceId;
                int countryId = 0;
                List<UpdateArticleTags> articleTags;
                List<UpdateRecompanies> articleCompanies;
                List<UpdateLocations> articleLocations;
                db_obj.getArticleDetails(aId, out content, out sourceTypeId, out sourceId,
                    out articleTags, out articleCompanies, out articleLocations);
                if (content == "")
                    continue;

                if (sourceTypeId == 2)
                    countryId = db_obj.getCountryIdByCompany(sourceId);

                article a = new article();
                a.content = content;
                if (!updateWithReEventParser(a, countryId, sourceTypeId == 2 ? sourceId : 0))
                {
                    db_obj.deleteArticle(aId);
                    this.Invoke((MethodInvoker)delegate
                    {
                        progressBar1.Value = ++i;
                        lbl_message2.Text = "Article " + i.ToString() + " from " + cnt.ToString() + " was deleted";
                    });
                    return;
                }

                List<UpdateArticleTags> tagsToDelete = new List<UpdateArticleTags>();
                List<UpdateArticleTags> tagsToAppend = new List<UpdateArticleTags>();

                foreach (int nt in a.eventIds)
                {
                    bool found = false;
                    foreach (UpdateArticleTags ot in articleTags)
                    {
                        if (nt == ot.Id)
                        {
                            ot.isUsed = true;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        articleTags.Add(new UpdateArticleTags { Id = 0, ArticleTagTypeValue = nt, isUsed = true });
                    }
                }

                foreach (int pt in a.propertyIds)
                {
                    bool found = false;
                    foreach (UpdateArticleTags ot in articleTags)
                    {
                        if (pt == ot.Id)
                        {
                            ot.isUsed = true;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        articleTags.Add(new UpdateArticleTags { Id = 0, ArticleTagTypeValue = pt, isUsed = true });
                    }
                }

                foreach (int nc in a.companyIds)
                {
                    bool found = false;
                    foreach (UpdateRecompanies oc in articleCompanies)
                    {
                        if (nc == oc.Id)
                        {
                            oc.isUsed = true;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        articleCompanies.Add(new UpdateRecompanies { Id = 0, ReCompanyId = nc, isUsed = true });
                    }
                }

                foreach (GeoLocation nl in a.locationIds)
                {
                    bool found = false;
                    foreach (UpdateLocations ol in articleLocations)
                    {
                        if (nl.locationId == ol.LocationId && nl.locationType == ol.LocationTypeId)
                        {
                            ol.isUsed = true;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        articleLocations.Add(new UpdateLocations { Id = 0, LocationTypeId = nl.locationType, LocationId = nl.locationId, isUsed = true });
                    }
                }
                db_obj.updateArticle(aId, a, articleTags, articleCompanies, articleLocations);
                this.Invoke((MethodInvoker)delegate
                {
                    progressBar1.Value = ++i;
                    lbl_message2.Text = "Article " + i.ToString() + " from " + cnt.ToString() + " was updated";
                });
            }
            reEventsParser.writeToLog("Update finished");
            runScannerDB();
        }
    }

    public class UpdateArticleTags
    {
        public long Id;
        public int ArticleTagTypeValue;
        public bool isUsed;
    }
    public class UpdateRecompanies
    {
        public long Id;
        public int ReCompanyId;
        public bool isUsed;
    }
    public class UpdateLocations
    {
        public long Id;
        public int LocationTypeId;
        public int LocationId;
        public bool isUsed;
    }

}
