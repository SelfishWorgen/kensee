using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;
using System.Net;
using utl;

namespace ReEvents
{
    public partial class ReEventsParser
    {
        public string status;
        public delegate void PerformProgress(string url, int n, int count);
        public delegate void WriteLineToResult(string subject, string content);

        DetectEvents eventDetection;
        DetectLocations locationDetection;
        DetectProperties propertiesDetection;
        DetectCompanies companiesDetection;
        DetectMoney moneyDetection;
        DetectArea areaDetection;
        DetectDate dateDetection;
        ArticleContent articleContent;

        // other:
        public static TokenParser tokenParser;
        public ReEventOptions opt;
        public SaveLog saveLog;

        //Access
        public List<int> EventIds { get { return eventDetection.eventIds; } }
        public List<int> CompanyIds { get { return companiesDetection.companyIds; } }
        public List<int> PropertyIds { get { return propertiesDetection.propertyIds; } }
        public List<GeoLocation> LocationIds { get { return locationDetection.getLocationIds(); } }
        public List<HierarchicalEvent> HierarchicalEvents;
        public string Snippet { get; set; }

        public ReEventsParser()
        {
            status = "";
            saveLog = null; //fill in Init
            opt = new ReEventOptions();

            eventDetection = new DetectEvents();
            locationDetection = new DetectLocations();
            companiesDetection = new DetectCompanies();
            propertiesDetection = new DetectProperties();
            articleContent = new ArticleContent();
            moneyDetection = new DetectMoney();
            areaDetection = new DetectArea();
            dateDetection = new DetectDate();

            HierarchicalEvents = new List<HierarchicalEvent>();
            Snippet = "";
        }

        private void clearResults()
        {
            eventDetection.clear();
            locationDetection.clear();
            companiesDetection.clear();
            propertiesDetection.clear();
            articleContent.clear();
            Snippet = "";
            HierarchicalEvents = new List<HierarchicalEvent>();
        }

        public void Init(List<ReEventsCompanyName> lcmn,
            List<ReEventsTagTypeValues> lettv,
            List<ReEventsCountryName> lcntrn,
            List<ReEventsStateName> lsn,
            List<ReEventsCityName> lcn,
            List<ReEventsCountry> lcntr,
            List<ReEventsState> ls,
            List<ReEventsCity> lcc,
            List<ReEventsLocType> llt,
            List<ReEventsPropertyKeyword> lpk,
            List<ReEventsRegionName> lrn,
            List<ReEventsPropertyName> lpn)
        {
            if (opt.optionsResult.Length > 0)
            {
                status = opt.optionsResult; // files are missing 
                return;
            }
            saveLog = new SaveLog(opt.FilesPath);
            saveLog.addLine("Start work", true);
            saveLog.addLine("ReEvent parser - initialization", false);
            tokenParser = new TokenParser(opt.FilesPath);

            companiesDetection.Init(tokenParser, lcmn);
            eventDetection.Init(opt.definitionsFileName, lettv);
            articleContent.Init(opt.FilesPath, saveLog);

            string streetSuffixFile = Path.Combine(opt.FilesPath, "street suffix abbreviations - short.txt");
            locationDetection.Init(tokenParser, lcntrn, lsn, lcn, lcntr, ls, lcc, llt, lrn, streetSuffixFile);
            propertiesDetection.Init(tokenParser, lpk, lpn);
        }

        public void analyzeByContent(string html, int countryId, int companyId, DateTime dt, string title)
        {
            extractInfoFromText(false, html, countryId, companyId, dt, title);
        }

        public void analyzeByUrl(string url, int countryId, int companyId, DateTime dt, string title)
        {
            extractInfoFromText(true, url, countryId, companyId, dt, title);
        }

        private void extractInfoFromText(bool needToReadByUrl, string x, int countryId, int companyId, DateTime dt, string title)
        {
            if (status.Length > 0)
                return;
            clearResults();
            articleContent.articleDate = dt;
            articleContent.predefinedTitle = title;
            if (needToReadByUrl)
                articleContent.readByUrl(x);
            else
                articleContent.readExistingContent(x);
            companiesDetection.predefinedCompanyId = companyId;
            locationDetection.predefinedCountryId = countryId;

            if (articleContent.articleSentencies.Count == 0)
                return;
            for (int i = 0; i < articleContent.articleSentencies.Count; i++)
            {
                if (articleContent.articleSentencies[i].isStartIgnoredText)
                    break;
                if (!articleContent.articleSentencies[i].isIgnored)
                    extractCompaniesFromSentence(articleContent.articleSentencies[i]);
            }
            findCompanyNameNotDB();
            bool companyWasFound = companiesDetection.companyIds.Count > 0 ||
                companiesDetection.predefinedCompanyId > 0 ||
                !String.IsNullOrEmpty(companiesDetection.companyNotInDB);
            for (int i = 0; i < articleContent.articleSentencies.Count; i++)
            {
                if (articleContent.articleSentencies[i].isStartIgnoredText)
                    break;
                if (!articleContent.articleSentencies[i].isIgnored)
                    extractInfoFromSentence(articleContent.articleSentencies[i], companyWasFound);
            }
            if (companyWasFound && articleContent.articleSentencies[0].hasSpecialTitleKeywords())
            {
                // it is financial report
                eventDetection.setFinancialEvent(articleContent.articleSentencies[0].txt);
                int cmid = companiesDetection.companyIds.Count > 0 ? companiesDetection.companyIds[0] : companiesDetection.predefinedCompanyId;
                int cId = companiesDetection.getCountryId(cmid);
                locationDetection.addCountry(cId);
            }
            else
            {
                eventDetection.getEvents();
                locationDetection.getLocations();
            }
            articleContent.propagateInfoForHierarchicalEvent(companiesDetection.predefinedCompanyId,
                companiesDetection.getCompanyName(companiesDetection.predefinedCompanyId),
                companiesDetection.companyNotInDB);
            fillHierarchicalEvents();
            Snippet = articleContent.getSnippet(eventDetection.getSentenceNumbersForEvent());
        }

        private void extractCompaniesFromSentence(ContentSentence sentence)
        {
            List<Token> tokens = tokenParser.GetTokensForText(sentence.txt);
            if (tokens.Count <= 2)
            {
                sentence.isIgnored = true;
                return;
            }
            companiesDetection.extractCompanyFromTokenList(tokens, sentence);
        }

        private void extractInfoFromSentence(ContentSentence sentence, bool companyWasFound)
        {
            List<Token> tokens = tokenParser.GetTokensForText(sentence.txt);
            List<Token> stemmedTokens = new List<Token>();
            foreach (Token t in tokens)
            {
                string s = Stemmer.Convert(t.token.ToLower());
                Token newToken = new Token(s, t.tag1, t.tag2, t.tag3, t.tokenStart, t.end_offset);
                stemmedTokens.Add(newToken);
            }
            eventDetection.extractEventFromTokenList(tokens, stemmedTokens, sentence, companyWasFound);
            locationDetection.extractLocationsFromTokenList(tokens, sentence);
            propertiesDetection.extractPropertyFromTokenList(tokens, stemmedTokens, sentence);
            if (sentence.eventIds.Count > 0)
            {
                moneyDetection.extractPriceFromTokenList(tokens, sentence);
                areaDetection.extractAreaFromTokenList(tokens, sentence);
            }
            dateDetection.extractDateFromTokenList(tokens, sentence);
        }

        public string Content()
        {
            return articleContent.Content();
        }

        public string ShortContent()
        {
            return articleContent.ShortContent();
        }

        public void setAnotherDefinitionFile(string definitionFile)
        {
            opt.definitionsFileName = definitionFile;
        }

        public void writeToLog(string text)
        {
            saveLog.addLine(text, false);
        }

        public void writeArticleToResultLog(int articleNum, string url, string title, DateTime dt, string shortContent,
            int sourceTypeId, string sourceTypeName, int sourceId, string sourceName)
        {
            saveLog.articleNumber = articleNum;
            saveLog.addResultEmptyLine();
            saveLog.addResultLine("url", url);
            saveLog.addResultLine("title", title);
            saveLog.addResultLine("Article date - db", dt.ToString("yyyy-MM-dd"));
            saveLog.addResultLine("short content - db", shortContent);
            saveLog.addResultLine("source type - db", sourceTypeId.ToString() + "(" + sourceTypeName + ")");
            saveLog.addResultLine("source - db", sourceId.ToString() + "(" + sourceName + ")");
            articleContent.writeContentToResult(saveLog.addResultLine);
            eventDetection.writeEventsToResult(saveLog.addResultLine);
            saveLog.addResultLine("snippet - db", Snippet);
            locationDetection.writeLocationsToResult(saveLog.addResultLine);
            companiesDetection.writeCompaniesToResult(saveLog.addResultLine);
            propertiesDetection.writePropertiesToResult(saveLog.addResultLine);
            //articleContent.writeHierarchicalEventsToResult(saveLog.addResultLine); //to remove
            writeHierarchicalEventsToResult(saveLog.addResultLine);
        }

        public void writeArticleSummaryToResultLog(string summary)
        {
            saveLog.addResultSummaryLine(summary);
        }

        public void writeArticleSentimentToResultLog(double value, string name, string log)
        {
            string s = log.Replace("\n", " | ");
            saveLog.addResultLine("Sentiment keywords", s);
            saveLog.addResultLine("sentiment - db", name + "(" + value.ToString() + ")");
        }

        public bool isRealEstateArticle(out string reason)
        {
            //reason = articleContent.foundForbiddenWords();
            reason = "";
            if (reason.Length > 0)
                return false;
            if (eventDetection.hasProperties())
                return true;
            if (companiesDetection.companyIds.Count > 0)
                return true;
            reason = "No one property from definition file and no one company from recompanies table was found";
            return false;
        }

        void fillHierarchicalEvents()  
        {
            for (int i = 0; i < articleContent.articleSentencies.Count; i++)
            {
                ContentSentence s = articleContent.articleSentencies[i];
                if (s.isIgnored)
                    continue;
                if (s.isStartIgnoredText)
                    break;
                if (s.propertyIds.Count == 0)
                    s.propertyIds.Add(0);           // for properties that found in the def file and not found in db
                foreach (int eventId in s.eventIds)
                {
                    if (s.locations == null || s.locations.Count == 0)
                        continue;
                    foreach (ResultLocation loc in s.locations)
                    {
                        foreach (int propertyId in s.propertyIds)
                        {
                            HierarchicalEvent he = new HierarchicalEvent(s.number, eventId, loc, s.companyNames, propertyId, s.areaStr, s.priceStr, s.dateStr);
                            bool isDuplicate = false;
                            foreach (HierarchicalEvent h in HierarchicalEvents)
                            {
                                if (h.isEqual(he))
                                {
                                    isDuplicate = true;
                                    break;
                                }
                            }
                            if (!isDuplicate)
                                HierarchicalEvents.Add(he);
                        }
                    }
                }
            }
        }

        void writeHierarchicalEventsToResult(WriteLineToResult writeLineToResult)
        {
            foreach (HierarchicalEvent he in HierarchicalEvents)
            {
                string res = "{" + he.sentenceNumber.ToString() + "} ";
                res += eventDetection.getEventName(he.eventId);
                //Property
                res += " | Property type: ";
                res += he.propertyId > 0 ? propertiesDetection.getPropertyName(he.propertyId) : "N/A"; 
                //bool first = true;
                //if (he.propertyIds.Count == 0) //should not be ?
                //{
                //    res += " | Property type: N/A";
                //}
                //else
                //{
                //    foreach (int propertyId in he.propertyIds)
                //    {
                //        if (first)
                //            res += " | Property type: ";
                //        else
                //            res += ", ";
                //        first = false;
                //        res += propertiesDetection.getPropertyName(propertyId);
                //    }
                //}
                //Location
                res += " | Location: ";
                if (he.location == null)
                {
                    res += "N/A";
                }
                else
                {
                    if (he.location.cityId > 0)
                    {
                        res += locationDetection.getCityName(he.location.cityId);
                        res += ", ";
                    }
                    res += locationDetection.getCountryName(he.location.countryId);
                }
                //Price:
                res += " | Price: ";
                res += he.priceStr == "" ? "N/A" : he.priceStr;
                //Area:
                res += " | Area: ";
                res += he.areaStr == "" ? "N/A" : he.areaStr;
                //Date:
                res += " | Date: ";
                res += he.dateStr == "" ? "N/A" : he.dateStr;
                //Company
                res += " | Company: ";
                res += he.companyNamesStr == "" ? "N/A" : he.companyNamesStr;
                writeLineToResult("Hierarchical event - db", res);
            }
        }

        private void findCompanyNameNotDB()
        {
            if (companiesDetection.companyIds.Count == 0 && companiesDetection.predefinedCompanyId <= 0)
            {
                List<ContentSentence> l = articleContent.getSentencesForAdditionalCompany();
                foreach (ContentSentence s in l)
                {
                    companiesDetection.extractCompanyNotInDB(tokenParser.GetTokensForText(s.txt), s);
                }
            }
        }

        public string tempCreateExcelFileForGraph(List<GraphEventData> evs, List<GraphSentimentData> sts, List<ReEventsTagTypeValues> tps) 
        {
            TempWriteToExcel t = new TempWriteToExcel();
            t.buildExcelGraph(evs, sts, tps, opt.FilesPath);
            return "";
        }

    }

    public class GeoLocation
    {
        public int locationType;
        public int locationId;
    }



}
