using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace utl
{
    public class Utils
    {
        public delegate void PerformProgress(int shift, string fileName, int n);
        public delegate void PerformParseUrl(string url, string fileName, string indicatorName, int shift);
        public static string allUnitsRegExpr = "\\b(tons?|mtco2(e|\\(e\\))|mt|metric ton(s|nes)( co2e)?|gj|tj|gigajoules|mwh|gwh|m3|litres|cubic meters|gallons)(\\b|[\\d])";

        public static int extractYear(List<ParsingRow> rows, int i, int j, int pos)
        {
            var str = rows[i].Cells[j].text;
            int year = Utils.extractCurrentGlobalYear(str);
            if (year != 0)
                return year;
            str = str.Substring(pos, str.Length - pos);
            var arr = str.Split(new char[] { ' ' });
            var wordsCount = 0;
            var i1 = i;
            var j1 = j;
            var k = 0;
            while (wordsCount++ <= 30)
            {
                if (k >= arr.Length)
                {
                    j1++;
                    if (j1 >= rows[i1].Cells.Count)
                    {
                        i1++;
                        if (i1 >= rows.Count)
                            break;
                        j1 = 0;
                    }
                    str = rows[i1].Cells[j1].text;
                    arr = str.Split(new char[] { ' ' });
                    k = 0;
                }
                year = Utils.extractCurrentGlobalYear(arr[k]);
                if (year != 0)
                    return year;
                k++;
            }
            str = rows[i].Cells[j].text.Substring(0, pos);
            arr = str.Split(new char[] { ' ' });
            wordsCount = 0;
            i1 = i;
            j1 = j;
            k = arr.Length - 1;
            while (wordsCount++ <= 30)
            {
                if (k < 0)
                {
                    j1--;
                    if (j1 < 0)
                    {
                        i1--;
                        if (i1 < 0)
                            break;
                        j1 = rows[i1].Cells.Count - 1;
                    }
                    str = rows[i1].Cells[j1].text;
                    arr = str.Split(new char[] { ' ' });
                    k = arr.Length - 1;
                }
                year = Utils.extractCurrentGlobalYear(arr[k]);
                if (year != 0)
                    return year;
                k--;
            }
            return 0;
        }

        public static int extractCurrentGlobalYear(string str)
        {
            Regex regexYearFileName = new Regex("20[01][0-9]", RegexOptions.None);
            MatchCollection yf = regexYearFileName.Matches(str);
            if (yf.Count != 0)
            {
                return Convert.ToInt32(yf[yf.Count - 1].Value);
            }
            return 0;
        }

        public static string sortPositions(string pos1, string pos2)
        {
            string newPos = "";
            List<KeyValuePair<int, int>> posList = new List<KeyValuePair<int, int>>();
            var a = pos1.Split(new char[] { ',' });
            foreach (var b in a)
            {
                var c = b.Split(new char[] { ' ' });
                posList.Add(new KeyValuePair<int, int>(Convert.ToInt32(c[0]), Convert.ToInt32(c[1])));
            }
            a = pos2.Split(new char[] { ',' });
            foreach (var b in a)
            {
                var c = b.Split(new char[] { ' ' });
                posList.Add(new KeyValuePair<int, int>(Convert.ToInt32(c[0]), Convert.ToInt32(c[1])));
            } 
            posList.Sort(delegate(KeyValuePair<int, int> x1, KeyValuePair<int, int> y1)
            {
                return x1.Key.CompareTo(y1.Key);
            });
            foreach (var z in posList)
            {
                Utils.addPosition(ref newPos, z.Key, z.Value);
            }
            return newPos;
        }

        public static void addPosition(ref string positions, int pos, int len)
        {
            if (positions != "")
                positions += ",";
            positions += pos.ToString() + " " + len.ToString();
        }

        public static bool allAreUpper(string str)
        {
            for (int i = 0; i < str.Length; i++)
                if (str[i] != ' ' && str[i] != '(' && str[i] != ')' && (!Char.IsLetter(str[i]) || Char.IsLower(str[i])))
                    return false;
            return true;
        }

        public static bool allAreDigits(string str)
        {
            for (int i = 0; i < str.Length; i++)
                if (str[i] != ' ' && str[i] != '.' && str[i] != ',' && !Char.IsDigit(str[i]))
                    return false;
            return true;
        }

        public static int letterCount(string str)
        {
            int count = 0;
            for (int i = 0; i < str.Length; i++)
                if (Char.IsLetter(str[i]))
                    count++;
            return count;
        }

        static public bool canParseFile(string fileName)
        {
            if (Path.GetFileName(fileName).StartsWith("temp_"))
                return false;
            string nm = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));
            string ext = Path.GetExtension(fileName);
            switch (ext)
            {
                case ".txt":
                    if (File.Exists(nm + ".pdf"))
                        return false;
                    if (File.Exists(nm + ".html"))
                        return false;
                    if (File.Exists(nm + ".shtml"))
                        return false;
                    if (File.Exists(nm + ".htm"))
                        return false;
                    if (nm.EndsWith("_"))
                    {
                        if (File.Exists(nm.Substring(0, nm.Length - 1) + ".txt"))
                            return false;
                    }
                    if (File.Exists(nm))
                        return false;
                    if (File.Exists(nm + ".xlsx"))
                        return false;
                    if (File.Exists(nm + ".xls"))
                        return false;
                    return true;
                default:
                    return true;
            }
        }

        static public bool canParseFolder(string folderName)
        {
            if (Path.GetFileName(folderName).ToLower() == "output")
                return false;
            if (File.Exists(folderName + ".pdf"))
                return false;
            return true;
        }
    }
}
