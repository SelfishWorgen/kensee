using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using utl;
using System.Text.RegularExpressions;


namespace ReEvents
{
    public class ArticleContent
    {
        List<Sentence> sentences;   // via  HTML parser
        List<string> sentenceStrings; // via NBoilerPipe
        private string stringContent; // via NBoilerPipe
        private string stringTitle; // via NBoilerPipe
        private string downLoadFileName;
        private bool nBoilerPipeUsed;
        SaveLog saveLog;
        public List<ContentSentence> articleSentencies;
        private string filesPath;
        public DateTime articleDate;
        public string predefinedTitle;
        ContentSentence aboutSentence;

        public ArticleContent()
        {
            sentences = new List<Sentence>();
            sentenceStrings = new List<string>();
            articleSentencies = new List<ContentSentence>();
            stringContent = "";
            stringTitle = "";
            // Content reading result:
            nBoilerPipeUsed = false;
            downLoadFileName = "";
            predefinedTitle = "";
            aboutSentence = null;
        }

        public void clear()
        {
            // Content reading result:
            sentences.Clear();
            sentenceStrings.Clear();
            articleSentencies.Clear();
            stringContent = "";
            stringTitle = "";
            nBoilerPipeUsed = false;
            predefinedTitle = "";
            aboutSentence = null;
            // Remove additional files from read pdf to html:
            clearTmpFiles();
        }

        public void Init(string fp, SaveLog sl)
        {
            filesPath = fp;
            downLoadFileName = Path.Combine(filesPath, "TempPdf" + ".pdf");
            saveLog = sl;
        }

        public void readExistingContent(string html)
        {
            if (string.IsNullOrEmpty(html))
                return;
            var htmlReader = new HtmlReader();
            var rows = htmlReader.readHtml(html);
            parseRows(rows, null);
            createArticleSentencies();
        }

        public void readByUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return;
            getContentSentencies(url);
            createArticleSentencies();
        }


        private void getContentSentencies(string url)
        {
            url = url.Trim();
            string ext = Path.GetExtension(url).ToLower();
            try
            {
                if (ext.StartsWith(".pdf"))
                {
                    nBoilerPipeUsed = false;
                    downLoadPdf(url);
                }
                else
                {
                    nBoilerPipeUsed = true;
                    getTextByUrl(url);
                }
            }
            catch (Exception ex)
            {
                sentences.Clear();
                if (ex is IOException)
                    throw;
            }
        }

        private void downLoadPdf(string url)
        {
            int n = 0;
            try
            {
                WebClient webClient = new WebClient();
                byte[] downloadedBytes = webClient.DownloadData(url);
                n = 0;
                while (downloadedBytes.Length == 0)
                {
                    if (++n > 60)
                        break;
                    System.Threading.Thread.Sleep(2000);
                    downloadedBytes = webClient.DownloadData(url);
                }
                if (n > 60)
                {
                    //"Cannot download file"
                    n++;
                    return;
                }
                string str = System.Text.Encoding.UTF8.GetString(downloadedBytes, 0, 20);
                Stream file1 = File.Open(downLoadFileName, FileMode.OpenOrCreate);
                file1.Write(downloadedBytes, 0, downloadedBytes.Length);
                file1.Close();
            }
            catch (Exception ex)
            {
                n++;
                saveLog.addLine("Exception while download " + url + " " + ex.Message, false);
                return;
            }
            HtmlParser.currentFileName = downLoadFileName;
            HtmlParser.currentPageNumber = 0;
            var htmlParser = new HtmlParser(parseRows, null, false);
            htmlParser.parsePdf();
        }


        public void parseRows(List<ParsingRow> rows, Utils.PerformProgress performProgress)
        {
            if (rows.Count == 0)
                return;
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                row.notEmptyCell = -2;
                for (int j = 0; j < row.Cells.Count; j++)
                {
                    if (!string.IsNullOrWhiteSpace(row.Cells[j].text))
                    {
                        if (row.notEmptyCell >= 0)
                        {
                            row.notEmptyCell = -1;
                            break;
                        }
                        row.notEmptyCell = j;
                    }
                }
                if (row.notEmptyCell == -2)
                    row.notEmptyCell = 0;
            }

            int firstTableN = 0;
            int lastTableN = -1;
            while (true) // to look all tables
            {
                int lastTextN = lastTableN + 1;
                for (; lastTextN < rows.Count; lastTextN++)
                {
                    if (rows[lastTextN].Cells.Count > 1)
                    {
                        lastTextN--;
                        break;
                    }
                }
                if (lastTextN == rows.Count)
                    lastTextN = rows.Count - 1;
                if (lastTableN + 1 <= lastTextN) // parse text
                {
                    int i = firstTableN;
                    while (true)
                    {
                        int lastN = i + 1000;
                        if (lastN > lastTextN)
                            lastN = lastTextN;
                        if (performProgress != null)
                            performProgress((i * 100) / rows.Count, "", 0);
                        parseText(rows, i, lastN);
                        if (lastN == lastTextN)
                            break;
                        i = lastN + 1;
                    }
                }
                firstTableN = lastTextN + 1;
                if (firstTableN >= rows.Count)
                    break;
                lastTableN = firstTableN + 1;
                int cellCount = 0;
                while (true)
                {
                    for (; lastTableN < rows.Count && rows[lastTableN].Cells.Count > 1; lastTableN++)
                    {
                        if (rows[lastTableN].Cells.Count > cellCount)
                            cellCount = rows[lastTableN].Cells.Count;
                    }
                    if (lastTableN + 1 < rows.Count && rows[lastTableN + 1].Cells.Count > 1)
                    {
                        lastTableN += 1;
                        continue;
                    }
                    if (lastTableN + 2 < rows.Count && rows[lastTableN + 2].Cells.Count > 1)
                    {
                        lastTableN += 2;
                        continue;
                    }
                    break;
                }
                if (lastTableN >= rows.Count)
                    lastTableN = rows.Count - 1;
                else if (rows[lastTableN].Cells.Count <= 1)
                    lastTableN--;
                if (performProgress != null)
                    performProgress((firstTableN * 100) / rows.Count, "", 0);
                parseTable(rows, firstTableN, lastTableN);
                firstTableN = lastTableN + 1;
                if (firstTableN >= rows.Count)
                    break;
            }
        }

        void parseTable(List<ParsingRow> rows, int firstN, int lastN)
        {
            //TODO later
        }

        void parseText(List<ParsingRow> rows, int firstN, int lastN)
        {
            TextContainer txtCont = new TextContainer(rows, firstN, lastN);
            txtCont.extractText();
            extractSentences(txtCont, false);
        }

        void extractSentences(TextContainer txtCont, bool fromTable)
        {
            for (; !txtCont.Parsed; txtCont.shift())
            {
                var s1 = txtCont.CurrentStr.Trim();
                if (string.IsNullOrWhiteSpace(s1))
                    continue;
                if (s1.IndexOf(' ') == -1)
                    continue;
                string positions = "";
                ParsingCellTextFragment tf = txtCont.textFragment(0, ref positions);
                Sentence.SentenceContext sentenceContext = Sentence.SentenceContext.SC_text;
                if (fromTable)
                    sentenceContext = tf.tableHeader ? Sentence.SentenceContext.SC_tableHeader : Sentence.SentenceContext.SC_cell;
                else if (tf.title) // tf.header || (tf.fontSize >= ParsingCellTextFragment.fontSizeMin * 2 && ParsingCellTextFragment.fontSizeMin > 0))
                    sentenceContext = Sentence.SentenceContext.SC_title;
                Sentence snt = new Sentence(s1, 0, sentenceContext, tf.fontSize, positions, 0);
                sentences.Add(snt);
            }
        }

        private void getTextByUrl(string url)
        {
            try
            {
                WebRequest request = WebRequest.Create(url.Trim());
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                responseFromServer = responseFromServer.Replace("<li>", "*+#");
                responseFromServer = responseFromServer.Replace("</li>", "*-#");
                HtmlConverter.ConvertToTextAndTitle(responseFromServer, 0, out stringContent, out stringTitle);
                stringContent = WebUtility.HtmlDecode(stringContent);
                if (string.IsNullOrEmpty(stringContent.Replace("\n", " ")))
                {
                    nBoilerPipeUsed = false;
                }
                else if (!string.IsNullOrEmpty(stringTitle) && stringContent == stringTitle)
                {
                    nBoilerPipeUsed = false;
                }
                else
                {
                    convertTextToSentences(stringContent);
                    if (!string.IsNullOrEmpty(stringTitle))
                    {
                        stringTitle = WebUtility.HtmlDecode(stringTitle);
                        if (sentenceStrings[0].IndexOf(stringTitle) < 0 && stringTitle.IndexOf(sentenceStrings[0]) < 0)
                            sentenceStrings.Insert(0, stringTitle);
                    }
                }
            }
            catch
            {
                nBoilerPipeUsed = false;
                sentenceStrings.Clear();
            }

            if (!nBoilerPipeUsed)
            {
                try
                {
                    var htmlReader = new HtmlReader();
                    var rows = htmlReader.readUrl(url);
                    parseRows(rows, null);
                }
                catch
                {
                    sentences.Clear();
                }
                if (sentences.Count == 0)
                {
                    saveLog.addLine("Cannot extract context for " + url, false);
                }
            }
        }

        void convertTextToSentences(string txt)
        {
            int first = 0;
            int len = txt.Length;
            for (int i = 0; i < len; i++)
            {
                if (txt[i] == '\n')
                {
                    addSentenceString(txt, ref first, i);
                }
                else if (txt[i] == '|')
                {
                    addSentenceString(txt, ref first, i);
                    continue;
                }
                else if (i < len - 2 && txt[i] == '*' && txt[i + 1] == '-' && txt[i + 2] == '#') //"*-#", end of list
                {
                    addSentenceString(txt, ref first, i + 2);
                    continue;
                }
                else if (txt[i] != '.')
                    continue;
                else if (i >= 2 && txt[i - 1] == 'r' && txt[i - 2] == 'M')
                    continue;
                else if (i >= 2 && txt[i - 1] == 't' && txt[i - 2] == 'S')
                    continue;
                else if (i >= 1 && i < len - 1 && Char.IsUpper(txt[i - 1]) && Char.IsUpper(txt[i + 1]))  //TSX: CHP.UN
                    continue;

                if (i < len - 1 && txt[i + 1] == '/')
                {
                    addSentenceString(txt, ref first, i + 1);
                    continue;
                }

                if (i + 1 < len && txt[i + 1] == ',')
                    continue;
                if (i + 2 < len && txt[i + 1] == ' ' && Char.IsLower(txt[i + 2]))
                    continue;

                int j = i - 1;
                for (; j >= 0; j--)
                {
                    if (!Char.IsWhiteSpace(txt[j]) && txt[j] != '\"' && txt[j] != '”' && txt[j] != '“')
                        break;
                }
                if (j >= 0 && (Char.IsDigit(txt[j]) || Char.IsLower(txt[j]) || txt[j] == ')') ||
                    (j >= 2 && Char.IsUpper(txt[j]) && Char.IsUpper(txt[j - 1]) && Char.IsUpper(txt[j - 2])))
                {
                    j = i + 1;
                    for (; j < len; j++)
                    {
                        if (!Char.IsWhiteSpace(txt[j]) && txt[j] != '\"' && txt[j] != '”' && txt[j] != '“')
                            break;
                    }
                    if (j < len && Char.IsUpper(txt[j]))
                    {
                        addSentenceString(txt, ref first, i);
                        continue;
                    }
                }
            }
            if (first < len)
            {
                addSentenceString(txt, ref first, len - 1);
            }
        }

        void addSentenceString(string txt, ref int first, int i)
        {
            string s = txt.Substring(first, i - first + 1).Replace("\n", " ").Trim();
            if (s != "" && s.Length != 1)
            {
                sentenceStrings.Add(s);
            }
            first = i + 1;
        }

        private void clearTmpFiles()
        {
            FileInfo fi = new FileInfo(downLoadFileName);
            if (fi.Exists)
            {
                try
                {
                    fi.Delete();
                }
                catch (Exception ex)
                {
                    saveLog.addLine("Exception during delete of file " + fi.FullName + ": " + ex.Message, false);
                }
            }
            DirectoryInfo di = new DirectoryInfo(Path.Combine(filesPath, "TempPdf"));
            if (di.Exists)
            {
                FileInfo[] fs = di.GetFiles();
                foreach (FileInfo f in fs)
                {
                    try
                    {
                        f.Delete();
                    }
                    catch (Exception ex)
                    {
                        saveLog.addLine("Exception during delete of file " + f.FullName + ": " + ex.Message, false);
                    }
                }
                try
                {
                    di.Delete();
                }
                catch (Exception ex)
                {
                    saveLog.addLine("Exception during delete of directory " + di.FullName + ": " + ex.Message, false);
                }
            }
        }

        private void createArticleSentencies()
        {
            bool firstSensibleWasFound = false;

            if (!string.IsNullOrEmpty(predefinedTitle))
                predefinedTitle = WebUtility.HtmlDecode(predefinedTitle);
            int k = 0;
            if (!nBoilerPipeUsed)
            {
                if (sentences.Count == 0)
                    return;
                if (!string.IsNullOrEmpty(predefinedTitle))
                {
                    if (sentences[0].text.IndexOf(predefinedTitle, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        ContentSentence c = new ContentSentence(predefinedTitle, 0, articleDate);
                        articleSentencies.Add(c);
                        k = 1;
                    }
                    else if (sentences[0].text.IndexOf(predefinedTitle, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        sentences[0].text = predefinedTitle; //to exclude title + " - REIT Canada"
                    }
                }
                for (int i = 0; i < sentences.Count; i++)
                {
                    ContentSentence c = new ContentSentence(sentences[i].text, i + k, articleDate);
                    if (!firstSensibleWasFound && c.number != 0 && c.isSensible())
                    {
                        c.firstSensible = true;
                        firstSensibleWasFound = true;
                    }
                    if (c.isStartAboutSection)
                        aboutSentence = c;
                    articleSentencies.Add(c);
                    if (c.isStartIgnoredText)
                        break;
                }
            }
            else
            {
                if (sentenceStrings.Count == 0)
                    return;
                if (!string.IsNullOrEmpty(predefinedTitle))
                {
                    if (sentenceStrings[0].IndexOf(predefinedTitle, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        ContentSentence c = new ContentSentence(predefinedTitle, 0, articleDate);
                        articleSentencies.Add(c);
                        k = 1;
                    }
                    else if (sentenceStrings[0].IndexOf(predefinedTitle, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        sentenceStrings[0] = predefinedTitle;
                    }
                }
                int prevNumber = k - 1;
                bool endOfListElementWas = false;
                bool inListElement = false;
                ContentSentence prevSentence = null;
                ContentSentence parentSentence = null;

                for (int i = 0; i < sentenceStrings.Count; i++)
                {
                    ContentSentence c = new ContentSentence(sentenceStrings[i], i + k, articleDate);

                    if (sentenceStrings[i].IndexOf("*+#") >= 0)
                    {
                        inListElement = true;
                        if (!endOfListElementWas) // it is first element in list
                            parentSentence = prevSentence;
                        if (parentSentence != null) // can be if there was unpaired end of list
                            parentSentence.listElementsCount++;
                        c.txt = c.txt.Replace("*+#", "•");
                    }
                    else if (endOfListElementWas) // list is finished; ignore 
                    {
                        inListElement = false;
                        parentSentence = null;
                    }

                    if (sentenceStrings[i].IndexOf("*-#") >= 0)
                    {
                        endOfListElementWas = true;
                        c.txt = c.txt.Replace("*-#", "");
                    }
                    else if (!inListElement)
                        endOfListElementWas = false;

                    c.parentListNumber = parentSentence != null ? parentSentence.number : -1;
                   
                    if (!firstSensibleWasFound && c.number != 0 && c.isSensible())
                    {
                        c.firstSensible = true;
                        firstSensibleWasFound = true;
                    }
                    if (c.isStartAboutSection)
                        aboutSentence = c;
                    articleSentencies.Add(c);
                    prevSentence = c;
                    if (c.isStartIgnoredText)
                        break;
                }
            }
        }

        public string Content()
        {
            string str = "";
            bool first = true;
            foreach (ContentSentence s in articleSentencies)
            {
                if (!first)
                    str += " ";
                first = false;
                str += s.txt.Trim();
            }
            if (!string.IsNullOrEmpty(predefinedTitle) && !string.IsNullOrEmpty(str) && str.Length > predefinedTitle.Length)
            {
                string s1 = str.Substring(0, predefinedTitle.Length);
                if (s1 == predefinedTitle)
                    str = str.Substring(predefinedTitle.Length);
            }
            return str;
        }

        public string ShortContent()
        {
            string str = "";
            int count = 0;
            bool first = true;
            foreach (ContentSentence s in articleSentencies)
            {
                if (s.number == 0) //title
                    continue;
                if (!s.isSensible())
                    continue;
                if (s.isIgnored)
                    continue;
                if (!first && !s.eventWasFound)
                    continue;
                if (!string.IsNullOrEmpty(predefinedTitle) && s.txt.IndexOf(predefinedTitle, StringComparison.OrdinalIgnoreCase) == 0)
                    continue;

                string text = s.txt.Trim();
                string text1, text2;
                int ii = text.IndexOf("--");
                if (ii > 0 && ii + 4 < text.Length)
                {
                    text1 = text.Substring(ii + 2).Trim();
                    text2 = text.Substring(0, ii).Trim();
                    text = text1.Length > text2.Length ? text1 : text2;
                    if (!string.IsNullOrEmpty(predefinedTitle) && text.IndexOf(predefinedTitle, StringComparison.OrdinalIgnoreCase) >= 0)
                        continue;
                    // it is solution for strings like and  
                    //"DDR Declares Second Quarter 2015 Class J and Class K Preferred Share... -- BEACHWOOD  Ohio  June 12  2015 /PRNewswire/"
                    //"BEACHWOOD  Ohio  June 12  2015 /PRNewswire/ -- DDR Declares Second Quarter 2015 Class J and Class K Preferred Share"
                }
                if (!first)
                {
                    str += " ";
                    first = false;
                }
                str += text;
                count++;
                if (count == 2)
                    break;
            }
            return str;
        }

        public void writeContentToResult(ReEventsParser.WriteLineToResult writeLineToResult)
        {
            string str = "";
            for (int i = 0; i < articleSentencies.Count; i++)
            {
                ContentSentence articleSentence = articleSentencies[i];
                string ignoreSymbol = articleSentence.isIgnored || articleSentence.isStartIgnoredText ? "-" : "";
                str += " {" + i.ToString() + ignoreSymbol + "}: " + articleSentence.txt.Trim();
            }
            writeLineToResult("text", str);
        }

        public string foundForbiddenWords()
        {
            string res = "";
            for (int i = 0; i < articleSentencies.Count; i++)
            {
                ContentSentence articleSentence = articleSentencies[i];
                if (!articleSentence.isIgnored && !articleSentence.isStartIgnoredText)
                {
                    if (articleSentence.txt.IndexOf("homeowner", StringComparison.OrdinalIgnoreCase) > 0)
                        return "Forbidden word 'homeowner' was found in content";
                }
            }
            return res;
        }

        public string getSnippet(List<int> sentenceNumbers)
        {
            if (sentenceNumbers == null)
                return "";
            Regex regex = new Regex(@"\$|¥|£|€|₪|₽|₴|₩|₹");

            ContentSentence s1 = null; // first 
            ContentSentence s2 = null; // first sensible without currency
            ContentSentence s3 = null; // first sensible with currency

            foreach (int i in sentenceNumbers)
            {
                foreach (ContentSentence s in articleSentencies)
                {
                    if (s.number == i)
                    {
                        if (s1 == null)
                            s1 = s;
                        bool isSensible = s.isSensible();
                        if (s2 == null && isSensible)
                            s2 = s;
                        MatchCollection foundKeys = regex.Matches(s.txt);
                        if (foundKeys.Count > 0 && isSensible)
                            s3 = s;
                        break;
                    }
                    if (s3 != null)
                        break;
                }
            }
            string text = "";
            if (s3 != null)
                text = s3.txt;
            else if (s2 != null)
                text = s2.txt;
            else if (s1 != null)
                text = s1.txt;
            if (text != "")
            {
                int ii = text.IndexOf("--");
                if (ii > 0 && ii + 4 < text.Length)
                {
                    string text1 = text.Substring(ii + 2).Trim();
                    if (!ContentSentence.isSensibleText(text1))
                        text1 = text.Substring(0, ii);
                    text = text1;
                }
            }

            if (text == "")
            {
                text = predefinedTitle;
            }
            text = text.Replace("\"", "").Replace("(", "").Replace(")", "").Replace("//", "");
            return text;
        }

        public void propagateInfoForHierarchicalEvent(int predefinedCompanyId, string predefinedCompanyName, string companyNotInDB)
        {
            //List<int> countryIds = null;
            List<ResultLocation> locations = null;
            List<int> companyIds = null;
            List<string> companyNames = null;
            string dateStr = "";
            for (int i = 0; i < articleSentencies.Count; i++)
            {
                ContentSentence s = articleSentencies[i];
                if (s.isIgnored)
                    continue;
                if (s.isStartIgnoredText)
                    break;
                if (s.locations.Count != 0)
                {
                    if (locations == null) //there were no locations in previous sentencies
                    {
                        for (int j = 0; j < articleSentencies.Count; j++)
                        {
                            ContentSentence s1 = articleSentencies[j];
                            if (s1.isIgnored)
                                continue;
                            if (s1.number == s.number)
                                break;
                            s1.locations = s.locations;
                        }
                    }
                    locations = s.locations;
                }
                if (s.companyIds.Count != 0)
                {
                    companyIds = s.companyIds;
                    companyNames = s.companyNames;
                }
                if (s.dateStr != "")
                {
                    dateStr = s.dateStr;
                }
                if (s.eventIds.Count == 0)
                    continue;
                if (s.companyIds.Count == 0)
                {
                    if (companyIds != null)
                    {
                        s.companyIds = companyIds;
                        s.companyNames = companyNames;
                    }
                    else if (predefinedCompanyId != 0)
                    {
                        s.companyIds.Add(predefinedCompanyId);
                        s.companyNames.Add(predefinedCompanyName);
                    }
                    else if (!String.IsNullOrEmpty(companyNotInDB))
                    {
                        s.companyNames.Add(companyNotInDB);
                    }
                    else
                    {
                        s.companyNames.Clear(); //debugging
                    }
                }
                if (s.locations.Count == 0)
                {
                    if (locations != null)
                        s.locations = locations;
                }
                if (s.dateStr == "")
                {
                    if (dateStr != "")
                        s.dateStr = dateStr;
                    else
                        s.dateStr = this.articleDate.ToShortDateString();
                }
            }
        }


        public List<ContentSentence> getSentencesForAdditionalCompany()
        {
            List<ContentSentence> l = new List<ContentSentence>();

            if (aboutSentence != null)
            {
                l.Add(aboutSentence);
            }

            // previous solution was to find company in first 5 sentencies:

            //for (int i = 1; i < 5; i++)
            //{
            //    if (i >= articleSentencies.Count)
            //        break;
            //    l.Add(articleSentencies[i]);
            //}
            return l;

        }

    }


}

