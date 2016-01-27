using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace utl
{
    public class QOptions
    {
        public string UserName { get; set; }

        public int extractorId;
        public int distance_factor;
        public int score_norm_factor;
        public double field_min;
        public double score_min;
        public double title_weight;
        public double table_header_weight;
        public int skip_sent;
        public int min_window;
        public double digitRatioTableRemoveCell;
        public double digitRatioTableRemoveTable;
        public bool showMessageBox;
        public int table_reading_order;

        public string baseFolder { get; set; }
        public string definitionsFileName { get; set; }
        string filesPath = null;
        public string FilesPath
        {
            get
            {
                if (baseFolder == null)
                    return null;
                return filesPath == null ? baseFolder + "\\files" : filesPath;
            }
            set
            {
                if (value != null && FilesPath != value)
                    filesPath = value;
            }
        }

        public string SourceFileName { get; set; }
        public string SourceFolderFilesName { get; set; }
        public string SourceSiteName { get; set; }
        public int TypeOfSource { get; set; }

        public int UsePdf2Html { get; set; }
        public int SaveTempFile { get; set; }

        public string CompanyName { get; set; }
        public int FirstYear { get; set; }
        public int LastYear { get; set; }

        public int SaveAnalysisResult { get; set; }
        string analysisResultSuffix = null;
        public string AnalysisResultSuffix
        {
            get
            {
                if (analysisResultSuffix == null)
                {
                    return "qout";
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
                if (baseFolder != null)
                {
                    string folder = baseFolder + "\\data\\" + CompanyName + "\\output\\" + UserName;
                    return folder + "\\" + CompanyName + "_" + FirstYear.ToString() + "-" + LastYear.ToString() + "_" + UserName + "_" + AnalysisResultSuffix + ".xlsx";
                }
                return null;
            }
        }


        IniParser parser;
        string programName;
        string iniPath;

        public QOptions(string p)
        {
            programName = p;
            UserName = Environment.UserName;
            string fld = Path.GetDirectoryName(Application.ExecutablePath);
            iniPath = fld + "\\" + programName + "_" + UserName + ".ini";
            if (File.Exists(iniPath))
                parser = new IniParser(iniPath);
            else
                parser = new IniParser(fld + "\\" + programName + ".ini");
            extractorId = parser.GetIntSetting(programName, "extractorId", 0);
            distance_factor = parser.GetIntSetting(programName, "distance_factor", 3);
            score_norm_factor = parser.GetIntSetting(programName, "score_norm_factor", 2);
            field_min = parser.GetDoubleSetting(programName, "field_min", 0.34);
            score_min = parser.GetDoubleSetting(programName, "score_min", 0.5);
            title_weight = parser.GetDoubleSetting(programName, "title_weight", 3);
            table_header_weight = parser.GetDoubleSetting(programName, "table_header_weight", 3);
            table_reading_order = parser.GetIntSetting(programName, "table_reading_order", 0);
            skip_sent = parser.GetIntSetting(programName, "skip_sent", 1);
            min_window = parser.GetIntSetting(programName, "min_window", 2);
            digitRatioTableRemoveCell = parser.GetDoubleSetting(programName, "digitRatioTableRemoveCell", 0.5);
            digitRatioTableRemoveTable = parser.GetDoubleSetting(programName, "digitRatioTableRemoveTable", 0.7);
            showMessageBox = parser.GetIntSetting(programName, "showMessageBox", 0) == 1;

            SourceFileName = parser.GetSetting(programName, "SourceFileName");
            SourceFolderFilesName = parser.GetSetting(programName, "SourceFolderFilesName");
            SourceSiteName = parser.GetSetting(programName, "SourceSiteName");
            TypeOfSource = parser.GetIntSetting(programName, "TypeOfSource", 0);

            CompanyName = parser.GetSetting(programName, "CompanyName");
            FirstYear = parser.GetIntSetting(programName, "FirstYear", 0);
            LastYear = parser.GetIntSetting(programName, "LastYear", 0);

            baseFolder = parser.GetSetting(programName, "BaseFolder");
            definitionsFileName = parser.GetSetting(programName, "DefinitionsFileName");
            FilesPath = parser.GetSetting(programName, "FilesPath");

            AnalysisResultSuffix = parser.GetSetting(programName, "AnalysisResultSuffix");
            SaveAnalysisResult = parser.GetIntSetting(programName, "SaveAnalysisResult", 0);

            UsePdf2Html = parser.GetIntSetting(programName, "UsePdf2Html", 0);
            SaveTempFile = parser.GetIntSetting(programName, "SaveTempFile", 1);
        }

        public void SaveOptionsToIniFile()
        {
            parser.AddSetting(programName, "SourceFileName", SourceFileName);
            parser.AddSetting(programName, "SourceFolderFilesName", SourceFolderFilesName);
            parser.AddSetting(programName, "SourceSiteName", SourceSiteName);
            parser.AddSetting(programName, "TypeOfSource", TypeOfSource.ToString());

            parser.AddSetting(programName, "CompanyName", CompanyName);
            parser.AddSetting(programName, "FirstYear", FirstYear.ToString());
            parser.AddSetting(programName, "LastYear", LastYear.ToString());

            parser.AddSetting(programName, "FilesPath", FilesPath);
            parser.AddSetting(programName, "BaseFolder", baseFolder);
            parser.AddSetting(programName, "DefinitionsFileName", definitionsFileName);

            parser.AddSetting(programName, "SaveAnalysisResult", SaveAnalysisResult.ToString());
            if (analysisResultSuffix != null)
                parser.AddSetting(programName, "AnalysisResultFileName", analysisResultSuffix);

            parser.AddSetting(programName, "extractorId", extractorId.ToString());
            parser.AddSetting(programName, "distance_factor", distance_factor.ToString());
            parser.AddSetting(programName, "score_norm_factor", score_norm_factor.ToString());
            parser.AddSetting(programName, "field_min", field_min.ToString());
            parser.AddSetting(programName, "score_min", score_min.ToString());
            parser.AddSetting(programName, "title_weight", title_weight.ToString());
            parser.AddSetting(programName, "table_header_weight", table_header_weight.ToString());
            parser.AddSetting(programName, "table_reading_order", table_reading_order.ToString());
            parser.AddSetting(programName, "skip_sent", skip_sent.ToString());
            parser.AddSetting(programName, "min_window", min_window.ToString());
            parser.AddSetting(programName, "digitRatioTableRemoveCell", digitRatioTableRemoveCell.ToString());
            parser.AddSetting(programName, "digitRatioTableRemoveTable", digitRatioTableRemoveTable.ToString());
            parser.AddSetting(programName, "showMessageBox", showMessageBox ? "1" : "0");
            parser.AddSetting(programName, "UsePdf2Html", UsePdf2Html.ToString());
            parser.AddSetting(programName, "SaveTempFile", SaveTempFile.ToString());
            parser.SaveSettings(iniPath);
        }

        public bool filesExist()
        {
            if (baseFolder == null || definitionsFileName == null)
                return false;
            DirectoryInfo d = new DirectoryInfo(baseFolder);
            if (!d.Exists)
                return false;
            FileInfo f = new FileInfo(definitionsFileName);
            if (!f.Exists)
                return false;
            if (TypeOfSource == 0)
            {
                if (string.IsNullOrWhiteSpace(SourceFileName))
                    return false;
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
