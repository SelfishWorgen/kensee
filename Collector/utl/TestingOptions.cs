using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using utl;

namespace utl
{
    public class TestingOptions
    {
        public string UserName { get; set; }
        private string iniPath;
        public IniParser parser { get; set; }

        public string SourceFolder { get; set; }
        public int FirstYear { get; set; }
        public int LastYear { get; set; }
        public string ResultFolder { get; set; }
        public int CheckSingleYear { get; set; }
        public int SingleYear { get; set; }
        public int CheckSingleFile { get; set; }
        public string SingleFileName { get; set; }
        public int CheckSingleCompany { get; set; }
        public string SingleCompanyName { get; set; }
        public bool enable { get { return parser != null; } }

        public TestingOptions()
        {
            UserName = Environment.UserName;
            string fld = Path.GetDirectoryName(Application.ExecutablePath);
            iniPath = fld + "\\semint_testing_" + UserName + ".ini";
            if (File.Exists(iniPath))
                parser = new IniParser(iniPath);
            else if (File.Exists(fld + "\\semint_testing.ini"))
                parser = new IniParser(fld + "\\semint_testing.ini");
            else
                return;
            SourceFolder = parser.GetSetting("semint_testing", "SourceFolder");
            FirstYear = parser.GetIntSetting("semint_testing", "FirstYear", 0);
            LastYear = parser.GetIntSetting("semint_testing", "LastYear", 0);

            ResultFolder = parser.GetSetting("semint_testing", "ResultFolder");
            CheckSingleYear = parser.GetIntSetting("semint_testing", "CheckSingleYear", 0);
            SingleYear = parser.GetIntSetting("semint_testing", "SingleYear", 0);
            CheckSingleFile = parser.GetIntSetting("semint_testing", "CheckSingleFile", 0);
            SingleFileName = parser.GetSetting("semint_testing", "SingleFileName");
            CheckSingleCompany = parser.GetIntSetting("semint_testing", "CheckSingleCompany", 0);
            SingleCompanyName = parser.GetSetting("semint_testing", "SingleCompanyName");
        }

        public void SaveOptionsToIniFile()
        {
            parser.AddSetting("semint_testing", "SourceFolder", SourceFolder);
            parser.AddSetting("semint_testing", "FirstYear", FirstYear.ToString());
            parser.AddSetting("semint_testing", "LastYear", LastYear.ToString());

            parser.AddSetting("semint_testing", "ResultFolder", ResultFolder);
            parser.AddSetting("semint_testing", "CheckSingleYear", CheckSingleYear.ToString());
            parser.AddSetting("semint_testing", "SingleYear", SingleYear.ToString());
            parser.AddSetting("semint_testing", "CheckSingleFile", CheckSingleFile.ToString());
            parser.AddSetting("semint_testing", "SingleFileName", SingleFileName);
            parser.AddSetting("semint_testing", "CheckSingleCompany", CheckSingleCompany.ToString());
            parser.AddSetting("semint_testing", "SingleCompanyName", SingleCompanyName);

            parser.SaveSettings(iniPath);
        }

        public bool filesExist()
        {
            if (SourceFolder == null || ResultFolder == null)
                return false;
            DirectoryInfo d = new DirectoryInfo(SourceFolder);
            if (!d.Exists)
                return false;
            d = new DirectoryInfo(ResultFolder);
            if (!d.Exists)
                return false;
            if (CheckSingleFile > 0)
            {
                if (SingleFileName == null)
                    return false;
                FileInfo f = new FileInfo(SingleFileName);
                if (!f.Exists)
                    return false;
            }
            return true;
        }
    }
}
