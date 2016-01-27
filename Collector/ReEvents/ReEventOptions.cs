using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using utl;
using System.IO;
using System.Windows.Forms;

namespace ReEvents
{
    public class ReEventOptions
    {
        public string optionsResult { get; set; }
        public string definitionsFileName { get; set; }
        public string FilesPath { get; set; }
        public string iniPath { get; set; }

        public double sentiment_threshold { get; set; }

        string UserName;
        IniParser parser;
        
        public int showDialog {get; set;}
        public bool needShowOptionsDialog { get { return !checkOptions() || showDialog > 0; } }

        public ReEventOptions()
        {
            UserName = Environment.UserName;
            optionsResult = "";

            string fld = Path.GetDirectoryName(Application.ExecutablePath);
            iniPath = fld + "\\" + "Collector_" + UserName + ".ini";
            parser = null;
            if (File.Exists(iniPath))
                parser = new IniParser(iniPath);
            else if (File.Exists(fld + "\\" + "Collector.ini"))
                parser = new IniParser(fld + "\\" + "Collector.ini");
            if (parser != null)
            {
                showDialog = parser.GetIntSetting("collector", "ShowOptionsDialog", 0);
                FilesPath = parser.GetSetting("collector", "DataFolder");
                definitionsFileName = parser.GetSetting("collector", "DefinitionsFileName");
                sentiment_threshold = parser.GetDoubleSetting("collector", "Sentiment_threshold", 3);
            }
            else
            {
                showDialog = 0;
                definitionsFileName = @"..\Data\RE_Events.xlsx";
                FilesPath = @"..\Data";
                sentiment_threshold = 3;
            }
        }

        public bool checkOptions()
        {
            DirectoryInfo d = new DirectoryInfo(FilesPath);
            if (!d.Exists)
            {
                optionsResult = "Cannot find data directory";
                return false;
            }
            FileInfo f = new FileInfo(definitionsFileName);
            if (!f.Exists)
            {
                optionsResult = "Cannot find definition file";
                return false;
            }
            return true;
        }

        public void SaveOptionsToIniFile()
        {
            if (parser == null)
                parser = new IniParser(iniPath);

            parser.AddSetting("collector", "ShowOptionsDialog", showDialog.ToString());
            parser.AddSetting("collector", "DataFolder", FilesPath);
            parser.AddSetting("collector", "DefinitionsFileName", definitionsFileName);
            parser.AddSetting("collector", "Sentiment_threshold", sentiment_threshold.ToString());
            parser.SaveSettings(iniPath);
        }


    }
}
