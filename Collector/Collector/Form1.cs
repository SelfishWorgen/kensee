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
        public DateTime start_date = new DateTime();
        List<article> articleData = new List<article>();
        ReEventsParser reEventsParser = new ReEventsParser();
        Options optUI;
        bool updateSentiments;
        bool updateEvents;
        int firstSiteNumber;
        int lastSiteNumber;
        int articleNumber;
        int reCompanyType;
        int newsWebSiteType;
        int positiveSentimentType;
        int neutralSentimentType;
        int negativeSentimentType;

        static string[] hardCodedCompanyNames = 
        {
            "", //0
            "REIT Canada", //1  
            "PR newswire", //2
            "Plaza Retail REIT", //3
            "First Capital Realty Inc.", //4
            "CT REIT", //5
            "Riocan REIT", //6 
            "Crombie REIT", //7 
            "Choice Properties REIT", //8 
            "Calloway REIT", //9
            "Swiss Prime Site", //10 - what is it?
            "Citycon Oyj", //11
            "Echo Investment S. A.", //12 - not Active
            "Multiplan", //13 - we excluded it
            "BR Malls Participacoes SA", //14
            "Sonae Sierra Brasil SA", //15
            "Hufvudstaden AB", //16
            "Platzer Fastigheter Holding AB", //17
        };

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            initSentiment();
            initLangIdentifier();

            updateSentiments = false;
            updateEvents = false;
            firstSiteNumber = 1;
            lastSiteNumber = 100;
            articleNumber = 0;
            optUI = new Options(reEventsParser.opt);
            
            if (reEventsParser.opt.needShowOptionsDialog)
            {
                optUI.ShowDialog();
                if (!optUI.isOK)
                {
                    this.Close();
                    return;
                }
                updateEvents = optUI.updateEvents;
                updateSentiments = optUI.updateSentiments;
                firstSiteNumber = optUI.firstNumber;
                lastSiteNumber = optUI.lastNumber;
            }
            if (optUI.createExcelFile)
            {
                tempCreateExcelFileForGraph(); //TEMP
                this.Close();
                return;
            }
            getSourceTypes(out reCompanyType, out newsWebSiteType);
            getSentimentTypes(out positiveSentimentType, out neutralSentimentType, out negativeSentimentType);
            initReEventParser();

            int y = DateTime.Now.Year;
            int m = DateTime.Now.Month;
            start_date = DateTime.Now.AddMonths(-6);

            lbl_message.Text = "Starting soon...";

            readNewsWebsite();

            if (optUI.processSeparateUrl)
            {
                processSeparateUrl();
                MessageBox.Show("Ready");
                this.Close();
                return;
            }

            progressBar2.Maximum = 20 + webSiteInfo.Count;
            timer1.Interval = 1000 * 3;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            Thread callThread;
            if  (updateEvents || updateSentiments)
                callThread = new Thread(() => UpdateDb());
            else
                callThread = new Thread(() => GetNews());
            callThread.IsBackground = true;
            callThread.Start();
        }

        public DateTime getLatestDate(int sourceTypeId, int sourceId, int monthes)
        {
            DBConnect db_obj = new DBConnect("kensee");
            return db_obj.selectLatestDate(sourceTypeId, sourceId, monthes);
        }

        public void GetNews()
        {
            if (reEventsParser.status.Length > 0)
            {
                MessageBox.Show(reEventsParser.status);
                return;
            }

            if (optUI.processSeparateUrl)
            {
                processSeparateUrl();
                return;
            }
            string strToLog = getLogTitle("Get News");
            reEventsParser.writeToLog(strToLog);

            scrapeWebSites();

            //GetNews_1(1);
            GetNews_2(2);
            GetNews_3(3);
            GetNews_4(4);
            GetNews_5(5);
            GetNews_6(6);
            GetNews_7(7);
            GetNews_8(8);
            GetNews_9(9);
            GetNews_10(10);
            GetNews_11(11);
            GetNews_12(12);
            //                GetNews_13(13);
            GetNews_14(14);
            GetNews_15(15);
            GetNews_16(16);
            GetNews_17(17);
            GetNews_18(18);
            GetNews_19(19);
            GetNews_20(20);
            reEventsParser.writeToLog("");
            runScannerDB();
            this.Invoke((MethodInvoker)delegate
            {
                timer2.Interval = 1000 * 10;
                lbl_message.Text = "Finalizing...";
                timer2.Start();
            });
        }

        bool beforeStep(int stepNumber, int sourceTypeId, int sourceId, int period)
        {
            if (stepNumber < firstSiteNumber || stepNumber > lastSiteNumber)
                return false;
            string strSuffix = "th";
            if (stepNumber == 1)
                strSuffix = "st";
            else if (stepNumber == 2)
                strSuffix = "nd";
            else if (stepNumber == 3)
                strSuffix = "rd";

            this.Invoke((MethodInvoker)delegate
            {
                lbl_message.Text = "Scraping articles of " + stepNumber.ToString() + strSuffix + " Site...";
            });

            if (sourceTypeId == 2 && sourceId == 0) //bloomberg, reuters, yahoo
                start_date = DateTime.Now.AddMonths(-6);
            else
                start_date = getLatestDate(sourceTypeId, sourceId, period);
            articleData.Clear();
            reEventsParser.writeToLog("");
            reEventsParser.writeToLog("Step number " + stepNumber.ToString() + ". Collect articles");
            return true;
        }

        void afterStep(int stepNumber)
        {
            this.Invoke((MethodInvoker)delegate
            {
                progressBar2.Value = stepNumber;
            });
        }

        private int getSourceTypeId(int stepNumber, out int sourceId, out int countryId)
        {
            string nm = "";
            sourceId = 0;
            countryId = 0;
            if (stepNumber <= 17)
                nm = hardCodedCompanyNames[stepNumber];
            foreach (WebSiteInfo w in webSiteInfo)
            {
                if (w.name == nm)
                {
                    sourceId = w.id;
                    reEventsParser.writeToLog("Step number " + stepNumber.ToString() + ": " + nm + ", source type id = 1, source id = " + sourceId.ToString());
                    return 1;
                }
            }
            DBConnect db_obj = new DBConnect("kensee");
            int isActive = 0;
            sourceId = db_obj.searchSourceIdInRecompanies(nm, out isActive, out countryId);
            if (isActive == 0)
            {
                reEventsParser.writeToLog("Step number " + stepNumber.ToString() + ": " + nm + " is not active");
                sourceId = 0;
            }
            else if (sourceId == 0)
                reEventsParser.writeToLog("Step number " + stepNumber.ToString() + ": " + nm + " is not found");
            else
                reEventsParser.writeToLog("Step number " + stepNumber.ToString() + ": " + nm + ", source type id = 2, source id = " + sourceId.ToString());
            return 2;
        }

        void extractHrefAndTitle(HtmlAgilityPack.HtmlNode node, out string hRef, out string title)
        {
            hRef = "";
            title = "";

            if (node == null)
                return;

            HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();
            doc2.LoadHtml(node.InnerHtml);

            HtmlAgilityPack.HtmlNode aNode = doc2.DocumentNode.SelectSingleNode("//a[@class='ModuleHeadlineLink']");
            if (aNode == null)
                aNode = doc2.DocumentNode.SelectSingleNode("//a");
            if (aNode != null)
            {
                hRef = aNode.Attributes["href"].Value;
                if (aNode.Attributes["title"] != null)
                    title = aNode.Attributes["title"].Value;
                if (title == "")
                    title = aNode.InnerText.Trim();
            }
            title = WebUtility.HtmlDecode(title);
        }


        DateTime extractDateTime(HtmlAgilityPack.HtmlNode node)
        {
            DateTime dt = DateTime.MinValue;
            if (node == null)
                return dt;
            string s = node.InnerText.Trim();
            dt = dateFromString(s);
            if (dt == DateTime.MinValue)
            {
                string strRegValue = "20[0-3][0-9]";
                Regex regexYear = new Regex(strRegValue, RegexOptions.None);
                MatchCollection fe = regexYear.Matches(s);
                if (fe.Count > 0)
                {
                    int p = fe[0].Index + 4;
                    s = s.Substring(0, p);
                    dt = dateFromString(s);
                }
            }
            if (dt == DateTime.MinValue)
            {
                string strRegValue = @"\d\d?\s+(January|February|March|April|May|June|July|August|September|October|November|December)\s+20[0-3][0-9]";
                Regex regexYear = new Regex(strRegValue, RegexOptions.IgnoreCase);
                MatchCollection fe = regexYear.Matches(s);
                if (fe.Count > 0)
                {
                    int p = fe[0].Index;
                    s = s.Substring(p, fe[0].Length);
                    dt = dateFromString(s);
                }
            }
            return dt;
        }

        public void storeMySQL()
        {
            if (articleData.Count == 0)
            {
                reEventsParser.writeToLog("Articles were not found");
                return;
            }

            int type = articleData[0].sourceId;
            this.Invoke((MethodInvoker)delegate
            {
                lbl_message2.Text = "Saving into a database...";
            });

            reEventsParser.writeToLog(articleData.Count.ToString() + " were added");
            DBConnect db_obj = new DBConnect("kensee");
            foreach (article item in articleData)
            {
                if (item.dt > DateTime.Now)
                    continue;
                if (item.content.Length > 32000)
                    item.content = item.content.Substring(0, 32000);
                if (item.content_short.Length > 5000)
                    item.content_short = item.content_short.Substring(0, 5000);
                if (item.snippet.Length > 1000)
                    item.snippet = item.snippet.Substring(0, 1000);
                if (db_obj.CheckExist(item))
                {
                    reEventsParser.writeToLog("Article already exist: title = '" + item.title + "', date = " + item.dt.ToShortDateString() + ", url = " + item.href);
                    continue;
                }
                if (db_obj.CheckExistShortCnt(item))
                {
                    reEventsParser.writeToLog("Article already exist: content_short = '" + item.content_short + "', title = '" + item.title + "', date = " + item.dt.ToShortDateString() + ", url = " + item.href);
                    continue;
                }
                db_obj.storeArticleToDB(item);
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            Application.Exit();
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

        void initReEventParser()
        {
            if (!reEventsParser.opt.checkOptions())
            {
                MessageBox.Show(reEventsParser.opt.optionsResult);
                this.Close();
            }
            DBConnect db_obj = new DBConnect("kensee");
            List<ReEventsCompanyName> lcmn = db_obj.getCompanies();
            List<ReEventsTagTypeValues> lettv = db_obj.getEventTagTypeValues();
            List<ReEventsCountryName> lcntrn = db_obj.getCountryNames();
            List<ReEventsStateName> lsn = db_obj.getStateNames();
            List<ReEventsCityName> lc = db_obj.getCityNames();
            List<ReEventsCountry> lcntr = db_obj.getCountryRegionIds();
            List<ReEventsState> ls = db_obj.getStateCountryIds();
            List<ReEventsCity> lcc = db_obj.getCityCountryIds();
            List<ReEventsLocType> llt = db_obj.getLocationTypes();
            List<ReEventsPropertyKeyword> lpk = db_obj.getPropertyKeywords();
            List<ReEventsRegionName> lrn = db_obj.getRegionNames();
            List<ReEventsPropertyName> lpn = db_obj.getPropertyNames();
            reEventsParser.Init(lcmn, lettv, lcntrn, lsn, lc, lcntr, ls, lcc, llt, lpk, lrn, lpn);

            if (!string.IsNullOrEmpty(reEventsParser.status))
            {
                MessageBox.Show(reEventsParser.status);
                this.Close();
            }
        }

        public bool updateWithReEventParser(article a, int countryId, int companyId)
        {
            if (a.href.IndexOf("http://biz.yahoo.com/research/earncal") >= 0)
                return false;

            DBConnect db_obj = new DBConnect("kensee");
            if (db_obj.CheckExist(a))
            {
                reEventsParser.writeToLog("Article already exist: title = '"  + a.title  + "', date = " + a.dt.ToShortDateString() + ", url = " + a.href);
                return false;
            }

            if (a.content == "")
                reEventsParser.analyzeByUrl(a.href, countryId, companyId, a.dt, a.title);
            else
                reEventsParser.analyzeByContent(a.content, countryId, companyId, a.dt, a.title);
            a.content = reEventsParser.Content();
            if (a.content == "")
            {
                reEventsParser.writeToLog("Content was not exctracted " + a.href);
                return false;
            }
            a.companyIds = reEventsParser.CompanyIds;
            a.locationIds = reEventsParser.LocationIds;
            a.eventIds = reEventsParser.EventIds;
            a.propertyIds = reEventsParser.PropertyIds;
            a.hierarchicalEvents = reEventsParser.HierarchicalEvents;
            if (string.IsNullOrEmpty(a.content_short) || a.content_short == a.title)
                a.content_short = reEventsParser.ShortContent();
            a.content_short = a.content_short.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Trim();
            if (string.IsNullOrEmpty(a.content_short))
            {
                reEventsParser.writeToLog("Short Content was not exctracted " + a.href);
                return false;
            }
            if (!isSiteEnglish(a))
            {
                reEventsParser.writeToLog("Article was identified as non-english " + a.href);
                return false;
            }
            articleNumber++;
            string sourceName = "";
            string sourceTypeName = "";
            if (a.sourceTypeId == 1)
            {
                foreach (WebSiteInfo w in webSiteInfo)
                {
                    if (w.id == a.sourceId)
                    {
                        sourceName = w.name;
                        break;
                    }
                }
                sourceTypeName = "News Website";
            }
            else
            {
                sourceName = a.companyname;
                sourceTypeName = "RE Company";
            }

            if (a.eventIds.Count == 0)
                //if (a.eventIds.Count == 0 && a.propertyIds.Count == 0)
            {
                reEventsParser.writeArticleToResultLog(articleNumber, a.href, a.title, a.dt, a.content_short, a.sourceTypeId, sourceTypeName, a.sourceId, sourceName);
                reEventsParser.writeArticleSummaryToResultLog("Article has not any events");
                reEventsParser.writeToLog("Aricle has not events " + a.href);
                return false;
            }
            string reason = "";
            if (!reEventsParser.isRealEstateArticle(out reason))
            {
                reEventsParser.writeArticleToResultLog(articleNumber, a.href, a.title, a.dt, a.content_short, a.sourceTypeId, sourceTypeName, a.sourceId, sourceName);
                reEventsParser.writeArticleSummaryToResultLog("Article is not Real Estate - " + reason);
                reEventsParser.writeToLog("Article is not Real Estate " + a.href);
                return false;
            }
            if (companyId != 0 && (a.companyIds.Count == 0 || !a.companyIds.Contains(companyId)))
            {
                a.companyIds.Add(companyId);
            }
            reEventsParser.writeArticleToResultLog(articleNumber, a.href, a.title, a.dt, a.content_short, a.sourceTypeId, sourceTypeName, a.sourceId, sourceName);
            if (a.locationIds.Count == 0)
                reEventsParser.writeToLog("Article was added, but no one country was found " + a.href);
            a.snippet = reEventsParser.Snippet;
            string log;
            getSentiment(a, out log);
            if (a.sentiment == "Positive")
                a.sentimentTypeId = positiveSentimentType;
            else if (a.sentiment == "Neutral")
                a.sentimentTypeId = neutralSentimentType;
            else if (a.sentiment == "Negative")
                a.sentimentTypeId = negativeSentimentType;
            reEventsParser.writeArticleSentimentToResultLog(a.sentimentValue, a.sentiment, log);

            // TEMP : remove general property and open-close events
            List<int> notShowTags = db_obj.getNotShowTags();
            for (int k = 0; k < a.eventIds.Count; )
            {
                if (notShowTags.Contains(a.eventIds[k]))
                    a.eventIds.RemoveAt(k);
                k++;
            }
            for (int k = 0; k < a.propertyIds.Count; )
            {
                if (notShowTags.Contains(a.propertyIds[k]))
                    a.propertyIds.RemoveAt(k);
                k++;
            }
            return true;
        }

        List<companyTicker> getCompanyTickers(string nm)
        {
            DBConnect db_obj = new DBConnect("kensee");
            return db_obj.getTickers(nm);
        }

        void getSourceTypes(out int reCompanyType, out int newsWebSiteType)
        {
            reCompanyType = 2;
            newsWebSiteType = 1;
            DBConnect db_obj = new DBConnect("kensee");
            db_obj.getSourceTypes(out reCompanyType, out newsWebSiteType);
        }
        
        void getSentimentTypes(out int positiveType, out int neutralType, out int negativeType)
        {
            positiveType = 1;
            neutralType = 2;
            negativeType = 3; 
            DBConnect db_obj = new DBConnect("kensee");
            db_obj.getSentimentTypes(out positiveType, out neutralType, out negativeType);
        }

        string getLogTitle(string s)
        {
            string strToLog = s;
            strToLog += " for kensee database ";
            strToLog += " with definition file ";
            strToLog += Path.GetFileName(reEventsParser.opt.definitionsFileName);
            if (firstSiteNumber != 1 || lastSiteNumber != 100)
            {
                strToLog += ", site number " + firstSiteNumber.ToString() + " - " + lastSiteNumber.ToString();
            }
            return strToLog;
        }

        void processSeparateUrl()
        {
            if (reEventsParser.status.Length > 0)
            {
                MessageBox.Show(reEventsParser.status);
                return;
            }

            DateTime dt = DateTime.Now;
            if (!String.IsNullOrEmpty(optUI.separateDateStr))
            {
                dt = dateFromString(optUI.separateDateStr);
                if (dt == DateTime.MinValue)
                    dt = DateTime.Now;
            }
            int countryId = 0;
            int companyId = 0;
            if (!String.IsNullOrEmpty(optUI.separateCompany))
            {
                DBConnect db_obj = new DBConnect("kensee");
                int isActive = 0;
                companyId = db_obj.searchSourceIdInRecompanies(optUI.separateCompany, out isActive, out countryId);
            }

            reEventsParser.analyzeByUrl(optUI.separateURL, countryId, companyId, dt, optUI.separateTitle);
            reEventsParser.writeArticleToResultLog(1,
                optUI.separateURL,
                optUI.separateTitle,
                dt,
                reEventsParser.ShortContent(),
                0,
                "Process separate url",
                companyId,
                optUI.separateCompany);
            string log;
            article a = new article();
            a.content = reEventsParser.Content();
            a.title = optUI.separateTitle;
            getSentiment(a, out log);
            reEventsParser.writeArticleSentimentToResultLog(a.sentimentValue, a.sentiment, log);
        }

        private void tempCreateExcelFileForGraph()
        {
            DBConnect db_obj = new DBConnect("kensee");
            List<GraphEventData> evs = db_obj.tempGetEventDataForGraph(40);
            List<GraphSentimentData> sts = db_obj.tempGetSentimentDataForGraph(40);
            List<ReEventsTagTypeValues> tps = db_obj.getEventNames();
            string resStr = reEventsParser.tempCreateExcelFileForGraph(evs, sts, tps);
            if (resStr == "")
                resStr = "Ready";
            MessageBox.Show(resStr);
        }

    }

    public class article
    {
        public string title = "";
        public string href = "";
        public string content = "";
        public DateTime dt;
        public int sourceTypeId = -1;
        public int sourceId = -1;
        public string companyname = "";
        public string sentiment;
        public double sentimentValue;
        public List<int> eventIds;
        public List<GeoLocation> locationIds;
        public List<int> propertyIds;
        public List<int> companyIds;
        public string snippet;
        public string content_short;
        public int sentimentTypeId;
        public List<HierarchicalEvent> hierarchicalEvents;

        public article()
        {
            content_short = "";
            companyname = "";
            title = "";
            href = "";
            content = "";
            dt = DateTime.Now;
            sentiment = "";
            sentimentValue = 0;
        }
    }

    public class companyTicker
    {
        public int id;
        public string companyName;
        public string ticker;
        public string country;
        public int countryId;
        public string industry;
    }

}


