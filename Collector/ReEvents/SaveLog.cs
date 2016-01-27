using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ReEvents
{
    public class SaveLog
    {
        string logFileName;
        string resultFileName;
        public int articleNumber { get; set; }

        public SaveLog(string baseFolder)
        {
            string fld = Path.Combine(baseFolder, "log");
            if (!Directory.Exists(fld))
                Directory.CreateDirectory(fld);
            logFileName = Path.Combine(fld, "log.txt");
            string n = string.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
            resultFileName = Path.Combine(fld, "results-" + n + ".txt");
        }

        public void addLine(string text, bool isHeader)
        {
            System.IO.StreamWriter logFile = new System.IO.StreamWriter(logFileName, true);

            string s = "";
            if (isHeader)
            {
                logFile.WriteLine("");
                logFile.WriteLine("");
                s = "                   ";
            }
            string n = string.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
            logFile.WriteLine(s + n + " " + text);
            if (isHeader)
            {
                logFile.WriteLine("");
            }
            logFile.Close();
        }

        public void addResultLine(string subject, string cntnt)
        {
            System.IO.StreamWriter resultFile = new System.IO.StreamWriter(resultFileName, true);
            string ln = articleNumber < 10000 ? articleNumber.ToString("D4") : articleNumber.ToString();
            ln += "\t" + subject.ToUpper() + "\t" + cntnt;
            resultFile.WriteLine(ln);
            resultFile.Close();
        }
        public void addResultEmptyLine()
        {
            System.IO.StreamWriter resultFile = new System.IO.StreamWriter(resultFileName, true);
            resultFile.WriteLine("");
            resultFile.Close();
        }

        public void addResultSummaryLine(string summary)
        {
            System.IO.StreamWriter resultFile = new System.IO.StreamWriter(resultFileName, true);
            string ln = articleNumber < 10000 ? articleNumber.ToString("D4") : articleNumber.ToString();
            ln += "\t" + summary.ToUpper();
            resultFile.WriteLine(ln);
            resultFile.Close();
        }
    }
}
