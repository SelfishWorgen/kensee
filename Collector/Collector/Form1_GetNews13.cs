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
        public void GetNews_13(int stepNumber)
        {
            int sourceId = 0;
            int countryId = 0;
            int sourceTypeId = getSourceTypeId(stepNumber, out sourceId, out countryId);
            if (sourceId == 0)
                return;
            if (!beforeStep(stepNumber, sourceTypeId, sourceId, 6))
                return;

            //string url = "http://www.multiplan.com.br/main.jsp?lumChannelId=499497CB220B572B01223DF002230A59";
            string url = "http://www.multiplan.com.br/main.jsp?lumPageId=499497CB316771AE013194FB7C7F5307&lumA=1&lumII=499497CB316771AE013194FB7E0D5313&locale=en_US&doui_processActionId=setLocaleProcessAction";
            this.Invoke((MethodInvoker)delegate
            {
                progressBar1.Maximum = 0;
                progressBar1.Value = 0;
                lbl_message2.Text = "Connecting...";
            });


            HtmlAgilityPack.HtmlDocument doc;
            try
            {
                HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
                doc = web.Load(url);
            }
            catch
            {
                afterStep(stepNumber);
                return;
            }

            HtmlAgilityPack.HtmlNodeCollection hrefNodeColl = doc.DocumentNode.SelectNodes("//a[@class='Trebuchet Branco n14 bgClipping']");
            if (hrefNodeColl != null)
            {
                int cnt = hrefNodeColl.Count;
                this.Invoke((MethodInvoker)delegate
                {
                    progressBar1.Maximum = cnt;
                    progressBar1.Value = 0;
                    lbl_message2.Text = "0 of " + cnt + " Articles were fetched...";
                });

                for (int i = 0; i < cnt; i++)
                {
                    string href = "http://www.multiplan.com.br/" + hrefNodeColl[i].Attributes["href"].Value;
                    HtmlAgilityPack.HtmlNodeCollection x = hrefNodeColl[i].ChildNodes;
                    if (x == null)
                        continue;
                    HtmlAgilityPack.HtmlNode p = x.FindFirst("span");
                    if (p == null)
                        continue;
                    string dt = p.InnerText.Trim();
                    if (dt == "")
                        continue;
                    string title = hrefNodeColl[i].InnerText.Trim().Replace(dt, "").Trim();
                    dt = dt.Replace("::", "").Trim();
                    DateTime enteredDate = dateFromString(dt);
                    if (enteredDate < start_date)
                        break;
                    if (url_dict.ContainsKey(href))
                        continue;
                    url_dict.Add(href, false);
                    article obj = new article();
                    obj.dt = enteredDate;
                    obj.content = "";
                    obj.href = href;
                    obj.title = title;
                    obj.sourceId = sourceId;
                    obj.sourceTypeId = sourceTypeId;
                    obj.companyname = "Multiplan";

                    if (updateWithReEventParser(obj, countryId, sourceTypeId == 2 ? sourceId : 0))
                        articleData.Add(obj);
                    this.Invoke((MethodInvoker)delegate
                    {
                        progressBar1.Value = articleData.Count;
                        lbl_message2.Text = articleData.Count + " of " + cnt + " Articles were fetched...";
                    });
                }
            }

            storeMySQL();
            afterStep(stepNumber);

        }
    }
}
