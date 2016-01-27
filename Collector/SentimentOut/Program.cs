using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;

namespace SentimentOut
{
    class Program
    {
        static string baseFolder;
        static int sentiment_threshold = 3;

        static void Main(string[] args)
        {
            baseFolder = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            initSentiment();
            testSentiment(args[0], args[1]);
        }
        static Dictionary<string, int> sentiment_dict;
        static Dictionary<string, int> negating_list;

        static void initSentiment()
        {
            sentiment_dict = new Dictionary<string, int>();
            var weights = File.ReadAllLines(Path.Combine(baseFolder, "AFINN-111-V1.csv"));
            foreach (var x in weights)
            {
                var ax = x.Split(new char[] { ',' });
                string txt = ax[0].Trim();
                if (!sentiment_dict.ContainsKey(txt))
                    sentiment_dict.Add(ax[0].Trim(), Convert.ToInt32(ax[1]));
                else
                    sentiment_dict[ax[0].Trim()] = Convert.ToInt32(ax[1]);
            }
            negating_list = new Dictionary<string, int>();
            var negs = File.ReadAllLines(Path.Combine(baseFolder, "NegatingWordList.txt"));
            foreach (var x in negs)
            {
                negating_list.Add(x.Trim().Replace("'", "^"), 0);
            }
        }

        static int sentiment_AFFIN(string txt, out string log)
        {
            log = "";
            int sentiment_text = 0;
            byte[] tempBytes;
            tempBytes = System.Text.Encoding.GetEncoding("us-ascii").GetBytes(txt);
            txt = System.Text.Encoding.UTF8.GetString(tempBytes);
            txt = txt.Replace('.', ' ');
            txt = txt.Replace(',', ' ');
            txt = txt.Replace(';', ' ');
            txt = txt.Replace(':', ' ');
            txt = txt.Replace('*', ' ');
            txt = txt.Replace('*', ' ');
            txt = txt.Replace('-', ' ');
            txt = txt.Replace('?', ' ');
            txt = txt.Replace('\n', ' ');
            txt = txt.Replace("'", "^");
            var words = txt.Split(new char[] { ' ' });
            for (int j = 0; j < words.Length; j++)
            {
                var wordsj = words[j].ToLower();
                if (sentiment_dict.ContainsKey(wordsj))
                {
                    int sentiment_word = sentiment_dict[wordsj];
                    if (sentiment_word < 0)
                        sentiment_word -= 1;
                    if (j > 0)
                    {
                        int j1 = j - 1;
                        if (negating_list.ContainsKey(words[j1]))
                        {
                            log += "NEGATING " + words[j1] + " " + words[j] + " " + sentiment_word.ToString() + " => \n";
                            sentiment_word = -1 * sentiment_word;
                            log += sentiment_word.ToString() + "\n"; ;
                        }
                    }
                    log += words[j] + " " + sentiment_word.ToString() + " " + sentiment_text + "\n";
                    sentiment_text += sentiment_word;
                }
            }
            return sentiment_text;
        }

        static string getSentiment(string text, out double sentimentValue, out string log)
        {
            sentimentValue = sentiment_AFFIN(text, out log);
            if (sentimentValue > sentiment_threshold)
                return "Positive";
            else if (sentimentValue < -sentiment_threshold)
                return "Negative";
            return "Neutral";
        }

        static void testSentiment(string inputFileName, string outputFileName)
        {
            //string inputFileName = Path.Combine(baseFolder, "manualEventClsfSent.xlsx");
            //string outputFileName = Path.Combine(baseFolder, "sentiments.txt");
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(outputFileName))
            {
                file.WriteLine("");

                Excel.Application excelApp = null;
                Excel.Workbook excelWorkBook = null;
                try
                {
                    excelApp = new Excel.Application();
                    excelWorkBook = excelApp.Workbooks.Open(inputFileName);
                    foreach (Excel.Worksheet first_sheet in excelWorkBook.Sheets)
                    {
                        file.WriteLine("number of sheets " + excelWorkBook.Sheets.Count);
                        file.WriteLine("sheet name " + first_sheet.Name);
                        Excel.Range range = first_sheet.UsedRange;
                        object[,] values = (object[,])range.Value2;
                        int num_records = range.Rows.Count;
                        int num_fields = range.Columns.Count;
                        file.WriteLine("num_fields " + num_fields);
                        file.WriteLine("num_records " + num_records);
                        int missed_negative = 0;
                        int missed_positive = 0;
                        int missed_neutral = 0;
                        int negative = 0;
                        int positive = 0;
                        int neutral = 0;
                        int start = 1;
                        for (int i = start; i < num_records; i++)
                        {
                            file.WriteLine("\nRecord " + i);
                            //    cell = first_sheet.cell(i,3)
                            var text = values[i + 1, 4].ToString();

                            //    #sentiment manual tag
                            //    cell = first_sheet.cell(i,5)
                            var sentiment = values[i + 1, 6].ToString().Trim();
                            //    sentiment1 = sentiment.encode('ascii','ignore')
                            if (sentiment == "")
                                continue;
                            //        continue
                            file.WriteLine("======================================================================");
                            file.WriteLine("\nRecord " + i);
                            string log;
                            var sentiment_text = sentiment_AFFIN(text, out log);
                            if (!string.IsNullOrWhiteSpace(log))
                                file.WriteLine(log);
                            file.WriteLine("\n" + "Sentiment manual tag " + sentiment + " Sentiment detected " + sentiment_text + "\n");

                            if (sentiment == "Positive")
                            {
                                positive += 1;
                                if (sentiment_text < sentiment_threshold)
                                {
                                    missed_positive += 1;
                                    file.WriteLine("\nMISSED POSITIVE");
                                    file.WriteLine("_______________________________________________________________\n");
                                    file.WriteLine("\nRecord " + i);
                                    file.WriteLine(text);
                                }
                            }
                            if (sentiment == "Negative")
                            {
                                negative += 1;
                                if (sentiment_text > -sentiment_threshold)
                                {
                                    missed_negative += 1;
                                    file.WriteLine("\nMISSED NEGATIVE");
                                    file.WriteLine("_______________________________________________________________\n");
                                    file.WriteLine("\nRecord " + i);
                                    file.WriteLine(text);
                                    file.WriteLine("\n" + "Sentiment manual tag: " + sentiment + " SentimentScore = " + sentiment_text + "\n");
                                }
                            }
                            if (sentiment == "Neutral")
                            {
                                neutral += 1;
                                if (sentiment_text > sentiment_threshold || sentiment_text < -sentiment_threshold)
                                {
                                    missed_neutral += 1;
                                }
                            }
                        }
                        float recall_positive = 1 - missed_positive / (float)(positive);
                        file.WriteLine("positive " + positive + " missed_positive " + missed_positive + " recall_postive " + recall_positive);

                        float recall_neutral = 1 - missed_neutral / (float)(neutral);
                        file.WriteLine("neutral " + neutral + " missed_neutral " + missed_neutral + " recall_neutral " + recall_neutral);

                        float recall_negative = 1 - missed_negative / (float)(negative);
                        file.WriteLine("negative " + negative + " missed_negative " + missed_negative + " recall_negative " + recall_negative);
                    }
                    excelWorkBook.Close(false);
                    excelApp.Quit();
                    //Marshal.ReleaseComObject(book);
                    //Marshal.ReleaseComObject(excelWorkBook);
                    return;
                }
                catch (Exception ex)
                {
                    excelWorkBook.Close(false);
                    excelApp.Quit();
                    return;
                }
            }
        }
    }
}
