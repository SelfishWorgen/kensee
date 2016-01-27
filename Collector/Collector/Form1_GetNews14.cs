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
        public void GetNews_14(int stepNumber)
        {
            int sourceId = 0;
            int countryId = 0;
            int sourceTypeId = getSourceTypeId(stepNumber, out sourceId, out countryId);
            if (sourceId == 0)
                return;
            if (!beforeStep(stepNumber, sourceTypeId, sourceId, 6))
                return;

            int curYear = DateTime.Now.Year;
            bool stopFlag = false;
            for (int i = curYear; i >= 2006; i--)
            {
                string url = "http://ir.brmalls.com.br/conteudo_en.asp?idioma=1&conta=44&tipo=51047&id=0&submenu=0&img=0&ano=" + i;

                this.Invoke((MethodInvoker)delegate
                {
                    progressBar1.Maximum = curYear - 2006 + 1;
                    progressBar1.Value = curYear - i + 1;
                    lbl_message2.Text = "Articles of Year " + i + " fetching...";
                });

                HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc;
                try
                {
                    doc = web.Load(url);
                }
                catch (Exception ex)
                {
                    reEventsParser.writeToLog("Cannot load " + url + " ;" + ex.Message);
                    continue;
                }

                HtmlAgilityPack.HtmlNode divNode = doc.DocumentNode.SelectSingleNode("//div[@class='arquivos']");
                if (divNode == null)
                    continue;

                HtmlAgilityPack.HtmlDocument doc1 = new HtmlAgilityPack.HtmlDocument();
                doc1.LoadHtml(divNode.InnerHtml);

                HtmlAgilityPack.HtmlNodeCollection trColl = doc1.DocumentNode.SelectNodes("//tr");
                if (trColl == null)
                    continue;

                foreach (HtmlAgilityPack.HtmlNode tr in trColl)
                {
                    HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();
                    doc2.LoadHtml(tr.InnerHtml);

                    string dt = "";
                    HtmlAgilityPack.HtmlNode dtNode = doc2.DocumentNode.SelectSingleNode("//td[@class='tabelatx data']");
                    if (dtNode == null)
                        continue;

                    dt = dtNode.InnerText.Trim();
                    string[] pieces = dt.Split(new string[] { "/" }, StringSplitOptions.None);
                    dt = pieces[2] + "-" + pieces[0] + "-" + pieces[1];
                    DateTime enteredDate;
                    if (!DateTime.TryParseExact(dt, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out enteredDate))
                        continue;
                    if (enteredDate < start_date)
                    {
                        stopFlag = true;
                        break;
                    }

                    string href = "";
                    string title = "";
                    HtmlAgilityPack.HtmlNode hrefNode = doc2.DocumentNode.SelectSingleNode("//a");
                    if (hrefNode == null)
                        continue;

                    title = hrefNode.InnerText.Trim();
                    href = "http://ir.brmalls.com.br/" + hrefNode.Attributes["href"].Value;
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
                    obj.companyname = "BR Malls Participacoes";

                    if (updateWithReEventParser(obj, countryId, sourceTypeId == 2 ? sourceId : 0))
                        articleData.Add(obj);
                }

                if (stopFlag == true)
                    break;
            }

            storeMySQL();
            afterStep(stepNumber);
        }
    }
}
