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
    public class Options
    {
        public const string ContentOneSource = "Summary";
        public const string ContentThreeSources = "Three sources";
        public const string ContentShowAll = "Show All";
        public const string ContentNone = "None";

        public static string quant_Indicator_definition = "FINEX_INDICATORS_v03March2015_QANT";
        string analysisResultSuffix = null;
        string filesPath = null;
        string sedolFileName = null;
        string definitionsFileName = null;

        string mainTabContent = null;
        string sourceTabContent = null;
        string detailTabContent = null;

        public string UserName { get; set; }
        public string BaseFolder { get; set; }

        public string FilesPath 
        { 
            get
            {
                if (BaseFolder == null)
                    return null;
                return filesPath == null ? BaseFolder + "\\files" : filesPath;
            }
            set
            {
                if (value != null && FilesPath != value)
                    filesPath = value;
            }
        }

        public int UseSedolFileName { get; set; }
        public string SedolFileName
        {
            get
            {
                if (FilesPath == null)
                    return null;
                return sedolFileName == null ? FilesPath + "\\sciesg_companies_sedol_codes1.xlsx" : sedolFileName;
            }
            set
            {
                if (value != null && SedolFileName != value)
                    sedolFileName = value;
            }
        }
        public string DefinitionsFileName
        {
            get
            {
                if (FilesPath == null)
                    return null;
                if (definitionsFileName != null)
                    return definitionsFileName;
                string nm = FilesPath + "\\" + quant_Indicator_definition + "_" + UserName + ".xls";
                if (File.Exists(nm))
                    return nm;
                return FilesPath + "\\" + quant_Indicator_definition + ".xls";
            }
            set
            {
                if (value != null && DefinitionsFileName != value)
                    definitionsFileName = value;
            }
        }
        public string DefinitionsFileName4Save
        {
            get
            {
                if (definitionsFileName != null)
                    return definitionsFileName;
                return FilesPath + "\\" + quant_Indicator_definition + "_" + UserName + ".xls";
            }
        }

        public int MaxSearch { get; set; }

        public string SourceFileName { get; set; }
        public string SourceFolderFilesName { get; set; }
        public string SourceSiteName { get; set; }
        public int TypeOfSource { get; set; }

        public string CompanyName { get; set; }
        public string CompanyUrlName { get; set; }
        public int FirstYear { get; set; }
        public int LastYear { get; set; }

        public int SaveAnalysisResult { get; set; }

        public string AnalysisResultSuffix 
        { 
            get
            {
                if (analysisResultSuffix == null)
                {
                    return "out";
                }
                else
                {
                    return analysisResultSuffix;
                }
            }
            set
            {
                if (value != null && AnalysisResultSuffix != value)
                    analysisResultSuffix = value;
            }
        }
        public string AnalysisResultFileNamePath
        { 
            get
            {
                if (BaseFolder != null)
                {
                    string folder = BaseFolder + "\\data\\" + CompanyName + "\\output\\" + UserName;
                    return folder + "\\" + CompanyName + "_" + FirstYear.ToString() + "-" + LastYear.ToString() + "_" + UserName + "_" + AnalysisResultSuffix + ".xlsx";
                }
                return null;
            }
        }

        public string MainTabContent
        {
            get
            {
                if (mainTabContent == null)
                {
                    return "Summary";
                }
                else
                {
                    return mainTabContent;
                }
            }
            set
            {
                if (value != null && mainTabContent != value)
                    mainTabContent = value;
            }
        }
        public string SourceTabContent
        {
            get
            {
                if (sourceTabContent == null)
                {
                    return "Three sources";
                }
                else
                {
                    return sourceTabContent;
                }
            }
            set
            {
                if (value != null && sourceTabContent != value)
                    sourceTabContent = value;
            }
        }

        public string DetailTabContent
        {
            get
            {
                if (detailTabContent == null)
                {
                    return "Show All";
                }
                else
                {
                    return detailTabContent;
                }
            }
            set
            {
                if (value != null && detailTabContent != value)
                    detailTabContent = value;
            }
        }

        public int ShowOnlyFound { get; set; }
        public int DontShowNoUnit { get; set; }
        int separateAnalysis = 0;
        public bool SeparateAnalysis
        {
            get { return separateAnalysis != 0; }
            set { separateAnalysis = value ? 1 : 0; }
        }
        public string SearchEngine { get; set; }
        public int NumberOfProcesses { get; set; }
        public int UsePdf2Html { get; set; }
        public string Year_Priority { get; set; }

        IniParser parser;
        string iniPath;



        public Options()
        {
            UserName = Environment.UserName;
            string fld = Path.GetDirectoryName(Application.ExecutablePath);
            iniPath = fld + "\\semint_ui_" + UserName + ".ini";
            if (File.Exists(iniPath))
                parser = new IniParser(iniPath);
            else
                parser = new IniParser(fld + "\\semint_ui.ini");

            BaseFolder = parser.GetSetting("semint_ui", "BaseFolder");
            SedolFileName = parser.GetSetting("semint_ui", "SedolFileName");
            UseSedolFileName = parser.GetIntSetting("semint_ui", "UseSedolFileName", 1);
            DefinitionsFileName = parser.GetSetting("semint_ui", "DefinitionsFileName");
            FilesPath = parser.GetSetting("semint_ui", "FilesPath");

            SourceFileName = parser.GetSetting("semint_ui", "SourceFileName");
            SourceFolderFilesName = parser.GetSetting("semint_ui", "SourceFolderFilesName");
            SourceSiteName = parser.GetSetting("semint_ui", "SourceSiteName");
            TypeOfSource = parser.GetIntSetting("semint_ui", "TypeOfSource", 0);

            CompanyName = parser.GetSetting("semint_ui", "CompanyName");
            CompanyUrlName = parser.GetSetting("semint_ui", "CompanyUrlName");
            FirstYear = parser.GetIntSetting("semint_ui", "FirstYear", 0);
            LastYear = parser.GetIntSetting("semint_ui", "LastYear", 0);

            AnalysisResultSuffix = parser.GetSetting("semint_ui", "AnalysisResultSuffix");
            SaveAnalysisResult = parser.GetIntSetting("semint_ui", "SaveAnalysisResult", 0);

            MaxSearch = parser.GetIntSetting("semint_ui", "MaxSearch", 40);
            separateAnalysis = parser.GetIntSetting("semint_ui", "SeparateAnalisys", 0);
            SearchEngine = parser.GetSetting("semint_ui", "SearchEngine");
            if (string.IsNullOrEmpty(SearchEngine)) SearchEngine = "Google Python";
            NumberOfProcesses = parser.GetIntSetting("semint_ui", "NumberOfProcesses", 4);
            UsePdf2Html = parser.GetIntSetting("semint_ui", "UsePdf2Html", 0);
            Year_Priority = parser.GetSetting("semint_ui", "Year_Priority");
            if (string.IsNullOrEmpty(Year_Priority)) Year_Priority = "name";

            MainTabContent = parser.GetSetting("semint_ui", "MainTabContent");
            MainTabContent = MainTabContent.Substring(0, 1).ToUpper() + MainTabContent.Substring(1);
            SourceTabContent = parser.GetSetting("semint_ui", "SourceTabContent");
            SourceTabContent = SourceTabContent.Substring(0, 1).ToUpper() + SourceTabContent.Substring(1);
            DetailTabContent = parser.GetSetting("semint_ui", "DetailTabContent");
            DetailTabContent = DetailTabContent.Substring(0, 1).ToUpper() + DetailTabContent.Substring(1);
            ShowOnlyFound = parser.GetIntSetting("semint_ui", "ShowOnlyFound", 0);
            DontShowNoUnit = parser.GetIntSetting("semint_ui", "DontShowNoUnit", 0);
        }

        public void SaveOptionsToIniFile()
        {
            parser.AddSetting("semint_ui", "BaseFolder", BaseFolder.ToString());
            parser.AddSetting("semint_ui", "SedolFileName", SedolFileName);
            parser.AddSetting("semint_ui", "UseSedolFileName", UseSedolFileName.ToString());
            parser.AddSetting("semint_ui", "DefinitionsFileName", DefinitionsFileName);
            parser.AddSetting("semint_ui", "FilesPath", FilesPath);

            parser.AddSetting("semint_ui", "MaxSearch", MaxSearch.ToString());
            parser.AddSetting("semint_ui", "SearchEngine", SearchEngine);

            parser.AddSetting("semint_ui", "SourceFileName", SourceFileName);
            parser.AddSetting("semint_ui", "SourceFolderFilesName", SourceFolderFilesName);
            parser.AddSetting("semint_ui", "SourceSiteName", SourceSiteName);
            parser.AddSetting("semint_ui", "TypeOfSource", TypeOfSource.ToString());

            parser.AddSetting("semint_ui", "CompanyName", CompanyName);
            parser.AddSetting("semint_ui", "CompanyUrlName", CompanyUrlName);
            parser.AddSetting("semint_ui", "FirstYear", FirstYear.ToString());
            parser.AddSetting("semint_ui", "LastYear", LastYear.ToString());

            parser.AddSetting("semint_ui", "SaveAnalysisResult", SaveAnalysisResult.ToString());
            if (analysisResultSuffix != null)
                parser.AddSetting("semint_ui", "AnalysisResultFileName", analysisResultSuffix);

            parser.AddSetting("semint_ui", "MainTabContent", MainTabContent);
            parser.AddSetting("semint_ui", "SourceTabContent", SourceTabContent);
            parser.AddSetting("semint_ui", "DetailTabContent", DetailTabContent);
            parser.AddSetting("semint_ui", "ShowOnlyFound", ShowOnlyFound.ToString());
            parser.AddSetting("semint_ui", "DontShowNoUnit", DontShowNoUnit.ToString());

            parser.AddSetting("semint_ui", "SeparateAnalisys", separateAnalysis.ToString());
            parser.AddSetting("semint_ui", "NumberOfProcesses", NumberOfProcesses.ToString());
            parser.AddSetting("semint_ui", "Year_Priority", Year_Priority);

            parser.AddSetting("semint_ui", "UsePdf2Html", UsePdf2Html.ToString());

            parser.SaveSettings(iniPath);
        }

        public bool filesExist()
        {
            if (BaseFolder == null || DefinitionsFileName == null || SedolFileName == null || FilesPath == null)
                return false;
            DirectoryInfo d = new DirectoryInfo(BaseFolder);
            if (!d.Exists)
                return false;
            FileInfo f = new FileInfo(DefinitionsFileName);
            if (!f.Exists)
                return false;
            f = new FileInfo(SedolFileName);
            if (!f.Exists)
                return false;
            d = new DirectoryInfo(FilesPath);
            if (!d.Exists)
                return false;
            if (TypeOfSource == 0)
            {
                f = new FileInfo(SourceFileName);
                if (!f.Exists)
                    return false;
            }
            else if (TypeOfSource == 1)
            {
                d = new DirectoryInfo(SourceFolderFilesName);
                if (!d.Exists)
                    return false;
            }
            return true;
        }
    }
}
