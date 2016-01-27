using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Diagnostics;

namespace utl
{
    public class HtmlParser
    {
        public static string currentFileName;
        public static int currentPageNumber;
        public static int currentTableNumber;
        public static bool excelParsed;

        public delegate void PerformParseRows(List<ParsingRow> rows, Utils.PerformProgress performProgress);

        PerformParseRows performParseRows;
        Utils.PerformProgress pProgress;
        List<ParsingRow> resultRows;
        bool newParse;

        public HtmlParser(PerformParseRows perfParseRows, Utils.PerformProgress pPrgs, bool newParseMethod)
        {
            performParseRows = perfParseRows;
            newParse = newParseMethod;
            pProgress = pPrgs;
        }

        public void parsePdf()
        {
            string outDir = Path.Combine(Path.GetDirectoryName(HtmlParser.currentFileName), Path.GetFileNameWithoutExtension(HtmlParser.currentFileName));
            if (!Directory.Exists(outDir))
            {
                if (pProgress != null)
                    pProgress(-1, "Conveting to HTML from " + HtmlParser.currentFileName, 0);
                string arg = "\"" + HtmlParser.currentFileName + "\" \"" + outDir + "\"";
                string fullName = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "pdftohtml.exe");
                try
                {
                    using (var proc = Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = fullName,
                            Arguments = arg,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            WorkingDirectory = Directory.GetCurrentDirectory() + "\\",
                        }))
                    {
                        proc.WaitForExit();
                        proc.Close();
                    }
                }
                catch (Exception ex)
                {
                    string nn = ex.Message;
                }
            }
            if (pProgress != null)
                pProgress(-1, "Parsing of " + HtmlParser.currentFileName, 0);
            string[] filePaths = Directory.GetFiles(outDir, "*.html");
            for (int i = 1; i < filePaths.Length; i++)
            {
                if (pProgress != null)
                    pProgress(i * 100 / filePaths.Length, "", 0);
                HtmlParser.currentPageNumber = i;
                string nm = Path.Combine(outDir, "page" + i.ToString() + ".html");
                if (File.Exists(nm))
                    parse(nm);
            }
            if (pProgress != null)
                pProgress(100, "", 0);
        }

        public void parse(string fileName)
        {
            excelParsed = false;
            resultRows = new List<ParsingRow>();
            if (pProgress != null && HtmlParser.currentPageNumber == 0)
                pProgress(-1, "Parsing of " + fileName, 0);
            HtmlReader reader = new HtmlReader();
            var rows = reader.read(fileName);
            if (reader.absoluteAllCells)
            {
                if (newParse)
                    buildRegions1(rows);
                else
                    buildRegions(rows);
                performParseRows(resultRows, HtmlParser.currentPageNumber == 0 ? pProgress : null);
                resultRows.Clear();
            }
            else
            {
                performParseRows(rows, HtmlParser.currentPageNumber == 0 ? pProgress : null);
            }
             if (pProgress != null && HtmlParser.currentPageNumber == 0)
                pProgress(100, "", 0);
        }

        void buildRegions1(List<ParsingRow> rows)
        {
            List<CellInfo> cellInfo = new List<CellInfo>();
            for (int i = 0; i < rows.Count; i++)
            {
                var cell = rows[i].Cells[0];
                bool toAddNew = true;
                if (cell.text == "")
                    continue;
                int right1 = cell.left + cell.getTextLength();
                foreach (var info in cellInfo)
                {
                    if (compare(cell.left, info.left, cell.fontSize))
                    {
                        if (info.secondCell == null)
                            info.secondCell = cell;
                        else if (cell.top < info.secondCell.top)
                            info.secondCell = cell;
                        info.rowsCount++;
                        info.fontSize += cell.fontSize;
                        info.textSize += cell.text.Length;
                        if (cell.text.Length > info.maxCellTextSize)
                            info.maxCellTextSize = cell.text.Length;
                        if (right1 > info.right)
                            info.right = right1;
                        if (cell.left < info.left)
                        {
                            info.left = cell.left;
                        }
                        toAddNew = false;
                        info.lastRowN = i;
                        break;
                    }
                 }
                if (toAddNew)
                {
                    var info = new CellInfo(cell.left, right1, cell.fontSize, cell.text.Length, cell);
                    info.firstRowN = info.lastRowN = i;
                    cellInfo.Add(info);

                }
            }
            cellInfo.Sort(delegate(CellInfo x, CellInfo y)
            {
                return x.left.CompareTo(y.left);
            });
            for (int i = 1; i < cellInfo.Count; )
            {
                if (cellInfo[i].right <= cellInfo[i - 1].right || cellInfo[i].left <= cellInfo[i - 1].right)
                //if ((cellInfo[i].left - cellInfo[i - 1].right < (cellInfo[i].firstCell.fontSize + 2) * 2)
                //    || (cellInfo[i].right < cellInfo[i - 1].right))
                {
                    if (cellInfo[i].right > cellInfo[i - 1].right)
                        cellInfo[i - 1].right = cellInfo[i].right;
                    cellInfo[i - 1].rowsCount += cellInfo[i].rowsCount;
                    cellInfo[i - 1].fontSize += cellInfo[i].fontSize;
                    cellInfo[i - 1].textSize += cellInfo[i].textSize;
                    if (cellInfo[i - 1].maxCellTextSize < cellInfo[i].maxCellTextSize)
                        cellInfo[i - 1].maxCellTextSize = cellInfo[i].maxCellTextSize;
                    if (cellInfo[i-1].firstRowN > cellInfo[i].firstRowN)
                        cellInfo[i-1].firstRowN = cellInfo[i].firstRowN;
                    if (cellInfo[i-1].lastRowN < cellInfo[i].lastRowN)
                        cellInfo[i-1].lastRowN = cellInfo[i].lastRowN;
                    if (cellInfo[i - 1].firstCell.top > cellInfo[i].firstCell.top && cellsAreIntersected(cellInfo[i - 1].firstCell, cellInfo[i].firstCell))
                    {
                        if (cellInfo[i].secondCell == null || cellInfo[i].secondCell.top > cellInfo[i - 1].firstCell.top)
                            cellInfo[i - 1].secondCell = cellInfo[i - 1].firstCell;
                        else
                            cellInfo[i - 1].secondCell = cellInfo[i].secondCell;

                        cellInfo[i - 1].firstCell = cellInfo[i].firstCell;
                    }
                    else if (cellInfo[i - 1].secondCell == null || cellInfo[i - 1].secondCell.top > cellInfo[i].firstCell.top)
                        cellInfo[i - 1].secondCell = cellInfo[i].firstCell;
                    cellInfo.RemoveAt(i);
                }
                else if (cellInfo[i].left < cellInfo[i - 1].right && cellInfo[i].rowsCount == 1)
                {
                    cellInfo.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
            // to unify cells
            for (int i = 1; i < cellInfo.Count; )
            {
                int year = 0;
                if (cellInfo[i].firstCell != null)
                {
                    double dbl = 0;
                    string txt1 = cellInfo[i].secondCell == null ? "" : cellInfo[i].secondCell.text.Trim().ToLower();
                    string txt = cellInfo[i].firstCell.text.Trim().ToLower();
                    if ((txt.Length >= 4 && int.TryParse(txt.Substring(0,4), out year) && year > 1980 && year < 2020)
                    || (txt1.Length >= 4 && int.TryParse(txt1.Substring(0,4), out year) && year > 1980 && year < 2020)
                    || (i+1 < cellInfo.Count && (txt = cellInfo[i+1].firstCell.text.Trim().ToLower()).Length >= 4 && int.TryParse(txt.Substring(0,4), out year) && year > 1980 && year < 2020)
                    || txt == "unit" || txt.StartsWith("unit ")
                    || txt == "region"
 //                   || cellInfo[i].left - cellInfo[i - 1].right < (cellInfo[i].fontSize / cellInfo[i].rowsCount + 2) * 2 
                    || txt.StartsWith("years ended")
                    || txt.StartsWith("december")
                    || txt.StartsWith("march")
                    || txt.StartsWith("note")
                    || Double.TryParse(txt, out dbl))
                    {
                        cellInfo[i - 1].right = cellInfo[i].right;
                        cellInfo[i - 1].rowsCount += cellInfo[i].rowsCount;
                        cellInfo[i - 1].fontSize += cellInfo[i].fontSize;
                        cellInfo[i - 1].textSize += cellInfo[i].textSize;
                        if (cellInfo[i - 1].maxCellTextSize < cellInfo[i].maxCellTextSize)
                            cellInfo[i - 1].maxCellTextSize = cellInfo[i].maxCellTextSize;
                        if (cellInfo[i - 1].firstRowN > cellInfo[i].firstRowN)
                            cellInfo[i - 1].firstRowN = cellInfo[i].firstRowN;
                        if (cellInfo[i - 1].lastRowN < cellInfo[i].lastRowN)
                            cellInfo[i - 1].lastRowN = cellInfo[i].lastRowN;
                        if (cellInfo[i - 1].firstCell.top > cellInfo[i].firstCell.top)
                            cellInfo[i - 1].firstCell = cellInfo[i].firstCell;
                        cellInfo.RemoveAt(i);
                        continue;
                    }
                }
                i++;
            }

            for (int i = 0; i < cellInfo.Count; i++)
            {
                cellInfo[i].fontSize /= cellInfo[i].rowsCount;
                cellInfo[i].filling = (cellInfo[i].textSize * 100) / (cellInfo[i].rowsCount * cellInfo[i].maxCellTextSize);
            }
            int origCellCount = cellInfo.Count;
            while (cellInfo.Count > 1)
            {
                if (cellInfo[0].maxCellTextSize < 20)
                    break;
                removeAndParseColumn1(0, rows, cellInfo);
            }
            if (origCellCount != cellInfo.Count)
                buildRegions1(rows);
            else
            {
                List<RowInfo> rowsInfo = new List<RowInfo>();
                for (int i = 0; i < rows.Count; i++)
                {
                    var cell = rows[i].Cells[0];
                    bool found = false;
                    for (int j = 0; j < rowsInfo.Count; j++)
                    {
                        if (compareTop(cell.top, rowsInfo[j].top, cell.fontSize > 0 ? cell.fontSize / 2 + 1 : 5)) // second cell in row
                        {
                            if (rowsInfo[j].top > cell.top)
                            {
                                rowsInfo[j].firstCell = cell;
                                rowsInfo[j].top = cell.top;
                            }
                            rowsInfo[j].cellsCount++;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        rowsInfo.Add(new RowInfo { cellsCount = 1, top = cell.top, fontSize = cell.fontSize, firstCell = cell });
                }
                rowsInfo.Sort(delegate(RowInfo x, RowInfo y)
                {
                    return x.top.CompareTo(y.top);
                });
                int delta = 0;
                if (rowsInfo.Count > 1)
                {
                    for (int i = 1; i < rowsInfo.Count; i++)
                        delta += rowsInfo[i].top - rowsInfo[i - 1].top - rowsInfo[i - 1].firstCell.fontSize;
                    delta /= (rowsInfo.Count - 1);
                }
                int origRowsCount = rowsInfo.Count;
                for (int i = 1; i < rowsInfo.Count - 1; i++)
                {
                    if ((rowsInfo[i].top - rowsInfo[i - 1].top > (rowsInfo[i-1].firstCell.fontSize + delta) * 2
                        && rowsInfo[i].firstCell.fontSize > rowsInfo[i - 1].firstCell.fontSize) ||
                        (rowsInfo[i].top - rowsInfo[i - 1].top > (rowsInfo[i-1].firstCell.fontSize + delta) * 3))
                    {
                        removeAndParserows1(rows, rowsInfo[i].top - 3);
                        buildRegions1(rows);
                        return;
                    }
                }
                int rowsCount = rowsInfo.Count;
                for (int i = 0; i < rowsCount; i++)
                {
                    if (rowsInfo[i].cellsCount != 1 || (i > 0 && !cellsAreIntersected(rowsInfo[i - 1].firstCell, rowsInfo[i].firstCell)))
                    {
                        if (i > 1)
                        {
                            removeAndParserows1(rows, rowsInfo[i - 1].top + 3);
                            for (int j = 0; j < i; j++)
                                rowsInfo.RemoveAt(0);
                        }
                        break;
                    }
                }
                if (rows.Count == 0)
                    return;
                for (int i = rowsInfo.Count - 1; i > 0; i--)
                {
                    if (rowsInfo[i].cellsCount != 1)
                    {
                        if (i != rowsInfo.Count - 1)
                        {
                            removeAndParserows1(rows, rowsInfo[i].top + 3);
                            for (int j = 0; j < i+1; j++)
                                rowsInfo.RemoveAt(0);
                        }
                        break;
                    }
                }
                if (rows.Count == 0)
                    return;
                if (origRowsCount != rowsInfo.Count)
                    buildRegions1(rows);
                else
                    rebuildAndParseRows(ref rows);
            }
        }

        void buildRegions(List<ParsingRow> rows)
        {
            List<RowInfo> rowsInfo = new List<RowInfo>();
            for (int i = 0; i < rows.Count; i++)
            {
                var cell = rows[i].Cells[0];
                //if (firstMultyCellTop != 0 && lastMultyCellTop != 0 && cell.top >= firstMultyCellTop && cell.top <= lastMultyCellTop)
                //    continue;
                bool found = false;
                for (int j = 0; j < rowsInfo.Count; j++)
                {
                    if (compareTop(cell.top, rowsInfo[j].top, cell.fontSize > 0 ? cell.fontSize / 2 + 2 : 5)) // second cell in row
                    {
                        if (rowsInfo[j].top > cell.top)
                            rowsInfo[j].top = cell.top;
                        rowsInfo[j].cellsCount++;
                        found = true;
                        break;
                    }
                }
                if (!found)
                    rowsInfo.Add(new RowInfo { cellsCount = 1, top = cell.top, fontSize = cell.fontSize, firstCell = cell });
            }
            rowsInfo.Sort(delegate(RowInfo x, RowInfo y)
            {
                return x.top.CompareTo(y.top);
            });
            for (int i = 1; i < rowsInfo.Count - 1; i++)
            {
                if ((rowsInfo[i].cellsCount == 1 &&  rowsInfo[i].top - rowsInfo[i - 1].top > 50)
                    || (rowsInfo[i].cellsCount == 1 && rowsInfo[i].top - rowsInfo[i - 1].top >= 40 && rowsInfo[i].fontSize > rowsInfo[i-1].fontSize + 15) 
                    || (rowsInfo[i - 1].cellsCount == 1 && rowsInfo[i].top - rowsInfo[i - 1].top > 40 && rowsInfo[i-1].fontSize > rowsInfo[i].fontSize + 4)
                    || (rowsInfo[i].cellsCount == 1 && rowsInfo[i - 1].cellsCount == 1 && rowsInfo[i].top - rowsInfo[i - 1].top == rowsInfo[i+1].top - rowsInfo[i].top &&
                        rowsInfo[i].top - rowsInfo[i - 1].top > 17 && rowsInfo[i - 1].fontSize == rowsInfo[i].fontSize && rowsInfo[i + 1].fontSize > rowsInfo[i].fontSize)
                    || (rowsInfo[i - 1].cellsCount == 1 && rowsInfo[i].top - rowsInfo[i - 1].top > 30 && rowsInfo[i].fontSize > rowsInfo[i-1].fontSize + 3))
                    {
                    removeAndParserows(rows, rowsInfo[i].top - 3);
                    for (int j = 0; j < i; j++)
                        rowsInfo.RemoveAt(0);
                    i = 1;
                }
            }
            //            removeAndParserows(List<ParsingRow> rows, int top);
            int firstMultyCellTop = 0;
            int rowsCount = rowsInfo.Count;
            for (int i = 0; i < rowsCount; i++)
            {
                if (rowsInfo[i].cellsCount != 1)
                {
                    firstMultyCellTop = rowsInfo[i].top;
                    break;
                }
            }
            if (rowsCount > 2 && rowsInfo[rowsCount - 1].fontSize < rowsInfo[rowsCount - 2].fontSize && rowsInfo[rowsCount - 1].top - rowsInfo[rowsCount - 2].top > 30)
                rowsInfo.RemoveAt(rowsCount - 1);
            int lastMultyCellTop = 0;
            for (int i = rowsInfo.Count - 1; i > 0; i--)
            {
                if (rowsInfo[i].cellsCount != 1)
                {
                    lastMultyCellTop = rowsInfo[i].top + 4;
                    break;
                }
            }
            List<CellInfo> cellInfo = new List<CellInfo>();
            for (int i = 0; i < rows.Count; i++)
            {
                var cell = rows[i].Cells[0];
                bool toAddNew = true;
                if (cell.text == "" || cell.top < firstMultyCellTop || cell.top > lastMultyCellTop)
                    continue;
                bool special = false;
                var txt = cell.text;
                if (txt.Length == 4)
                {
                    int year = 0;
                    special = int.TryParse(txt, out year) && year > 2000 && year < 2020;
                }
                foreach (var info in cellInfo)
                {
                    if (cell.left == info.left)
                    {
                        info.rowsCount++;
                        info.fontSize += cell.fontSize;
                        info.textSize += cell.text.Length;
                        if (cell.text.Length > info.maxCellTextSize)
                            info.maxCellTextSize = cell.text.Length;
                        int right1 = cell.left + cell.getTextLength();
                        if (right1 > info.right)
                            info.right = right1;
                        toAddNew = false;
                        info.lastRowN = i;
                        if (info.firstCell.top > cell.top)
                            info.firstCell = cell;
                        if (info.secondCell == null || info.secondCell.top > cell.top)
                            info.secondCell = cell;
                        info.special |= special;
                        break;
                    }
                }
                if (toAddNew)
                {
                    int right1 = cell.left + cell.getTextLength();
                    int size = TextRenderer.MeasureText(cell.text, new Font("sans-serif", cell.fontSize, FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel)).Width;
                    int right = cell.left + size;
                    var info = new CellInfo(cell.left, right1, cell.fontSize, cell.text.Length, cell);
                    info.firstRowN = info.lastRowN = i;
                    info.special = special;
                    cellInfo.Add(info);

                }
            }
            cellInfo.Sort(delegate(CellInfo x, CellInfo y)
            {
                return x.left.CompareTo(y.left);
            });
            for (int i = 1; i < cellInfo.Count; )
            {
                if (cellInfo[i].right <= cellInfo[i - 1].right)
                {
                    ;
                 }
                else if (cellInfo[i].left <= cellInfo[i - 1].right)
                {
                    if (cellInfo[i].rowsCount > 2)
                        cellInfo[i - 1].right = cellInfo[i].right;
                }
                else
                {
                    i++;
                    continue;
                }
                cellInfo[i - 1].rowsCount += cellInfo[i].rowsCount;
                cellInfo[i - 1].fontSize += cellInfo[i].fontSize;
                cellInfo[i - 1].textSize += cellInfo[i].textSize;
                if (cellInfo[i - 1].maxCellTextSize < cellInfo[i].maxCellTextSize)
                    cellInfo[i - 1].maxCellTextSize = cellInfo[i].maxCellTextSize;
                if (cellInfo[i - 1].firstRowN > cellInfo[i].firstRowN)
                    cellInfo[i - 1].firstRowN = cellInfo[i].firstRowN;
                if (cellInfo[i - 1].lastRowN < cellInfo[i].lastRowN)
                    cellInfo[i - 1].lastRowN = cellInfo[i].lastRowN;
                if (cellInfo[i - 1].firstCell.top > cellInfo[i].firstCell.top)
                    cellInfo[i - 1].firstCell = cellInfo[i].firstCell;
                cellInfo[i - 1].special |= cellInfo[i].special;
                cellInfo.RemoveAt(i);
            }
            // to unify cells
            bool nextToUnion = false;
            for (int i = 1; i < cellInfo.Count; )
            {
                bool toUnion = nextToUnion | cellInfo[i].special;
                string txtc = cellInfo[i].firstCell.text.ToLower();
                if (txtc == "units" && i + 2 < cellInfo.Count && cellInfo[i + 2].firstCell != null)
                {
                    int year = 0;
                    string txt = cellInfo[i + 2].firstCell.text.Replace("*", "").Trim();
                    if (txt.Length >= 4)
                    {
                        nextToUnion = int.TryParse(txt.Substring(0, 4), out year) && year > 2000 && year < 2020;
                    }
                    else
                        nextToUnion = false;
                }
                else
                {
                    nextToUnion = false;
                }
                toUnion |= txtc == "unit" || txtc == "units" ||  txtc.StartsWith("unit ")
                    || (txtc.StartsWith("$") && Utils.letterCount(txtc) < 3)
                    || txtc == "region"
                    || txtc == "note"
                    || txtc == "notes" 
                    || cellInfo[i].left - cellInfo[i - 1].right < 8
                    || Utils.letterCount(txtc) == 0;
                if (!toUnion)
                {
                    int year = 0;
                    if (cellInfo[i].firstCell != null)
                    {
                        string txt = cellInfo[i].firstCell.text.Replace("*", "").Trim();
                        if (txt.Length >= 4 && txt.IndexOf("comp" ) == -1)
                        {
                            toUnion = int.TryParse(txt.Substring(0,4), out year) && year > 2000 && year < 2020;
                        }
                    }
                    if (!toUnion && cellInfo[i].secondCell != null)
                    {
                        string txt = cellInfo[i].secondCell.text.Replace("*", "").Trim();
                        if (txt.Length >= 4 && txt.IndexOf("comp") == -1)
                        {
                             toUnion = int.TryParse(txt.Substring(0, 4), out year) && year > 2000 && year < 2020;
                        }
                    }
                }
                if (toUnion)
                {
                    cellInfo[i - 1].right = cellInfo[i].right;
                    cellInfo[i - 1].rowsCount += cellInfo[i].rowsCount;
                    cellInfo[i - 1].fontSize += cellInfo[i].fontSize;
                    cellInfo[i - 1].textSize += cellInfo[i].textSize;
                    if (cellInfo[i - 1].maxCellTextSize < cellInfo[i].maxCellTextSize)
                        cellInfo[i - 1].maxCellTextSize = cellInfo[i].maxCellTextSize;
                    if (cellInfo[i - 1].firstRowN > cellInfo[i].firstRowN)
                        cellInfo[i - 1].firstRowN = cellInfo[i].firstRowN;
                    if (cellInfo[i - 1].lastRowN < cellInfo[i].lastRowN)
                        cellInfo[i - 1].lastRowN = cellInfo[i].lastRowN;
                    if (cellInfo[i - 1].firstCell.top > cellInfo[i].firstCell.top)
                        cellInfo[i - 1].firstCell = cellInfo[i].firstCell;
                    cellInfo.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            for (int i = 0; i < cellInfo.Count; )
            {
                cellInfo[i].fontSize /= cellInfo[i].rowsCount;
                cellInfo[i].filling = (cellInfo[i].textSize * 100) / (cellInfo[i].rowsCount * cellInfo[i].maxCellTextSize);
                if (cellInfo[i].rowsCount == 1)
                    cellInfo.RemoveAt(i);
                else
                    i++;
            }
            while (cellInfo.Count > 1)
            {
                if (cellInfo.Count == 2 && cellInfo[0].right < cellInfo[1].right / 2 && cellInfo[1].left > cellInfo[1].right / 2 &&
                    cellInfo[1].left - cellInfo[0].right > 120
                    && cellInfo[0].filling > 40 && cellInfo[1].filling > 40
                    && cellInfo[0].rowsCount > 4)
                {
                    removeAndParseColumn(0, rows, cellInfo);
                }
                else if (cellInfo.Count == 2 && cellInfo[0].filling > 52 && cellInfo[1].filling > 52 && cellInfo[1].fontSize >= cellInfo[0].fontSize)
                {
                    removeAndParseColumn(0, rows, cellInfo);
                }
                else if (cellInfo.Count == 2 && cellInfo[1].filling > 80 && cellInfo[1].fontSize > cellInfo[0].fontSize + 1)
                {
                    removeAndParseColumn(0, rows, cellInfo);
                }
                else if (cellInfo.Count == 3 && check3(cellInfo))
                {
                    removeAndParseColumn(0, rows, cellInfo);
                    removeAndParseColumn(0, rows, cellInfo);
                }
                else if (cellInfo.Count == 2 && check2(cellInfo))
                {
                    removeAndParseColumn(0, rows, cellInfo);
                }
                else if (cellInfo[cellInfo.Count - 1].left - cellInfo[cellInfo.Count - 2].right > 95) // new!!!
               {
                    removeAndParseColumn(cellInfo.Count - 2, rows, cellInfo);
                }
                else
                {
                    bool wasRemoved = false;
                    for (int i = 0; i < cellInfo.Count - 1; )
                    {
                        if (cellInfo[i].filling > 78 && cellInfo[i].rowsCount > 4)
                        {
                            removeAndParseColumn(i, rows, cellInfo);
                        }
                        else if (cellInfo[i].filling > 47 && cellInfo[i + 1].firstCell.fontSize > 10 && cellInfo[i].rowsCount > 4)
                        {
                            removeAndParseColumn(i, rows, cellInfo);
                        }
                        else if (cellInfo[i].filling > 60 && cellInfo[i].rowsCount > 4 && cellInfo[i + 1].rowsCount > 3 &&
                            cellInfo[i + 1].left - cellInfo[i].right > 20 && cellInfo[i].maxCellTextSize > 40)
                        {
                            removeAndParseColumn(i, rows, cellInfo);
                        }
                        else if (Utils.allAreUpper(cellInfo[i + 1].firstCell.text) && cellInfo[i + 1].firstCell.fontSize > cellInfo[i + 1].fontSize)
                        {
                            removeAndParseColumn(i, rows, cellInfo);
                        }
                        else if (cellInfo[i].firstCell.fontSize == cellInfo[i + 1].firstCell.fontSize && cellInfo[i].fontSize <= cellInfo[i].firstCell.fontSize - 3
                            && cellInfo[i + 1].fontSize <= cellInfo[i].firstCell.fontSize - 3)
                        {
                            removeAndParseColumn(i, rows, cellInfo);
                        }
                        else
                        {
                            i++;
                            continue;
                        }
                        i = 0;
                        wasRemoved = true;
                    }
                    if (!wasRemoved)
                        break;
                }
            }
            rebuildAndParseRows(ref rows);
        }

        bool check2(List<CellInfo> cellInfo)
        {
            int l1 = cellInfo[0].right - cellInfo[0].left;
            int l2 = cellInfo[1].right - cellInfo[1].left;
            return compare(l1, l2, 15);
        }

        bool check3(List<CellInfo> cellInfo)
        {
            int l1 = cellInfo[0].right - cellInfo[0].left;
            int l2 = cellInfo[1].right - cellInfo[1].left;
            int l3 = cellInfo[2].right - cellInfo[2].left;
            return compare(l1, l2, 13) && compare(l1, l3, 13) && compare(l2, l3, 13);
        }

        bool compare(int n1, int n2, int d)
        {
            if (n1 > n2)
                return n1 - n2 < d;
            return n2 - n1 < d;
        }

        void removeAndParserows(List<ParsingRow> rows, int top)
        {
            List<ParsingRow> rows1 = new List<ParsingRow>();
            for (int i = 0; i < rows.Count; )
            {
                var cell = rows[i].Cells[0];
                if (cell.top < top)
                {
                    rows1.Add(rows[i]);
                    rows.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
            buildRegions(rows1);
        }

        void removeAndParserows1(List<ParsingRow> rows, int top)
        {
            List<ParsingRow> rows1 = new List<ParsingRow>();
            for (int i = 0; i < rows.Count; )
            {
                var cell = rows[i].Cells[0];
                if (cell.top < top)
                {
                    rows1.Add(rows[i]);
                    rows.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
            buildRegions1(rows1);
        }

        void removeAndParseColumn(int col, List<ParsingRow> rows, List<CellInfo> cellInfo)
        {
            List<ParsingRow> rows1 = new List<ParsingRow>();
            for (int i = 0; i < rows.Count; )
            {
                var cell = rows[i].Cells[0];
                //if (cell.left >= cellInfo[col].left && cell.left <= cellInfo[col].right)
                if (cell.left <= cellInfo[col].right)
                {
                    rows1.Add(rows[i]);
                    rows.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
            buildRegions(rows1);
            for (int i = 0; i <= col; i++)
                cellInfo.RemoveAt(0);
 //           rebuildAndParseRows(ref rows1);
        }

        void removeAndParseColumn1(int col, List<ParsingRow> rows, List<CellInfo> cellInfo)
        {
            List<ParsingRow> rows1 = new List<ParsingRow>();
            for (int i = 0; i < rows.Count; )
            {
                var cell = rows[i].Cells[0];
                if (cell.left >= cellInfo[col].left && cell.left <= cellInfo[col].right)
                {
                    rows1.Add(rows[i]);
                    rows.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
            buildRegions1(rows1);
            cellInfo.RemoveAt(col);
        }

        void rebuildAndParseRows(ref List<ParsingRow> rows)
        {
            for (int i = 0 + 1; i <= rows.Count - 1; )
            {
                bool removed = false;
                var prevCell = rows[i - 1].Cells[rows[i - 1].Cells.Count - 1];
                if (rows[i].Cells.Count == 1 && rows[i].Cells[0].top != 0)
                {
                    if (compareTop(prevCell.top, rows[i].Cells[0].top, 3))
                    {
                        rows[i - 1].Cells.Add(rows[i].Cells[0]);
                        rows.RemoveAt(i);
                        removed = true;
                    }
                    else if (prevCell.top >= rows[i].Cells[0].top - 2)
                    {
                        for (int i1 = 0; i1 < i; i1++)
                        {
                            var cell = rows[i1].Cells[rows[i1].Cells.Count - 1];
                            if (compareTop(cell.top, rows[i].Cells[0].top, 3))
                            {
                                rows[i1].Cells.Add(rows[i].Cells[0]);
                                rows.RemoveAt(i);
                                removed = true;
                                break;
                            }
                            else if (cell.top >= rows[i].Cells[0].top - 2)
                            {
                                var row = rows[i];
                                rows.RemoveAt(i);
                                rows.Insert(i1, row);
                                break;
                            }
                        }
                    }
                }
                if (!removed)
                    i++;
            }
            for (int i = 1; i < rows.Count; i++)
            {
                if (rows[i].Cells.Count == 1 && rows[i - 1].Cells.Count != 1 && rows[i].Cells[0].fontSize <= rows[i - 1].Cells[0].fontSize / 2
                    && rows[i].Cells[0].top - rows[i-1].Cells[0].top > 30)
                {
                    List<ParsingRow> rows1 = new List<ParsingRow>();
                    for (int i1 = 0; i1 < i; i1++)
                    {
                        rows1.Add(rows[0]);
                        rows.RemoveAt(0);
                    }
                    correctColumns(ref rows1);
                    resultRows.AddRange(rows1);
                    rows1.Clear();
                    break;
                }
            }
            if (rows.Count != 0)
            {
                correctColumns(ref rows);
                resultRows.AddRange(rows);
                rows.Clear();
            }
        }

        void correctColumns(ref List<ParsingRow> rows)
        {
            if (rows.Count > 4 && rows[1].Cells.Count > 2 && rows[3].Cells.Count > 2 && rows[1].Cells[0].left > rows[3].Cells[0].left + 100)
            {
                var newCell = new ParsingCell(rows[3].Cells[0].left, rows[1].Cells[0].top);
                rows[1].InsertCell(0, newCell);
            }
            int maxLeft = 0;
            // correct columns
            List<int> columns = new List<int>();
            for (int i = 0; i <= rows.Count - 1; i++)
            {
                var row = rows[i];
                for (int j = 0; j < row.Cells.Count; j++)
                {
                    var cell = row.Cells[j];

                    if (maxLeft < cell.left)
                        maxLeft = cell.left;
                    if (j > columns.Count - 1)
                        columns.Add(0);
                    if (cell.left != 0 && (cell.left < columns[j] || columns[j] == 0))
                    {
                        if (j == 0 && columns[j] != 0 && cell.left < columns[j] - 300 )
                            columns.Insert(j, cell.left);
                        else
                            columns[j] = cell.left;
                    }
                }
            }
            if (maxLeft > columns[columns.Count-1] + 34)
                columns.Add(maxLeft);
            for (int i = 0; i < rows.Count; )
            {
                var row = rows[i];
                for (int j = 0; j < row.Cells.Count && j < columns.Count - 1; j++)
                {
                    var cell = row.Cells[j];
                    if (cell.left > columns[j + 1] - 13)
                    {
                        var newCell = new ParsingCell(columns[j], cell.top);
                        row.InsertCell(j, newCell);
                    }
                }
                if (row.Count < columns.Count)
                {
                    var cell = row.Cells[row.Count - 1];
                    var txt = cell.text;
                    if (txt.ToLower().StartsWith("unit "))
                    {
                        var arr = txt.Split(new char[] { ' ' });
                        if (row.Count + arr.Length - 1 <= columns.Count)
                        {
                            int pos = 0;
                            ParsingCell newCell = new ParsingCell(cell.left, cell.top);
                            newCell.addText(new ParsingCellTextFragment(arr[0], cell.getFilePosition(0), 0, ""));
                            row.Cells[row.Count - 1] = newCell;
                            for (int j = 1; j < arr.Length; j++)
                            {
                                pos += arr[j - 1].Length + 1;
                                newCell = new ParsingCell(cell.left, cell.top);
                                newCell.addText(new ParsingCellTextFragment(arr[j], cell.getFilePosition(pos), 0, ""));
                                row.AddCell(newCell);
                            }
                        }
                    }
                }
                if (i > 0 && rows[i].Cells.Count == 2 && !string.IsNullOrWhiteSpace(rows[i].Cells[0].text) && !string.IsNullOrWhiteSpace(rows[i].Cells[1].text)
                    && rows[i - 1].Cells.Count > 2
                    && !string.IsNullOrWhiteSpace(rows[i - 1].Cells[0].text) && !string.IsNullOrWhiteSpace(rows[i - 1].Cells[1].text)
                   && (i == rows.Count - 1 || rows[i + 1].Cells[0].top - rows[i].Cells[0].top > rows[i].Cells[0].top - rows[i - 1].Cells[0].top)
                    && Utils.letterCount(rows[i].Cells[1].text) * 100 / rows[i].Cells[1].text.Length > 60)
                {
                    if (rows[i - 1].Cells[1].textFragments.Count == 1 && rows[i - 1].Cells[1].textFragments[0].text.EndsWith(" N/A"))
                        rows[i - 1].Cells[1].textFragments[0].text = rows[i - 1].Cells[1].textFragments[0].text.Replace("N/A", "");
                    else
                        rows[i - 1].Cells[1].addText(" ");
                    rows[i - 1].Cells[0].addText(" ");
                    rows[i - 1].Cells[0].addTexts(rows[i].Cells[0].textFragments);
                    rows[i - 1].Cells[1].addTexts(rows[i].Cells[1].textFragments);
                    rows.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            for (int i = 1; i < rows.Count - 1; )
            {
                var row = rows[i];
                if (row.Count == 2 && rows[i - 1].Cells.Count >= 2 && string.IsNullOrWhiteSpace(row.Cells[0].text) && !string.IsNullOrWhiteSpace(row.Cells[1].text)
                    && !string.IsNullOrWhiteSpace(rows[i - 1].Cells[1].text))
                {
                    if (row.Cells[0].top - rows[i - 1].Cells[0].top + 2 < rows[i + 1].Cells[0].top - row.Cells[0].top)
                    {
                        if (rows[i - 1].Cells.Count >= 3 && string.IsNullOrWhiteSpace(rows[i - 1].Cells[2].text))
                        {
                             var cell = rows[i - 1].Cells[1];
                            string str = cell.text;
                            if (Char.IsDigit(str[str.Length - 1]))
                            {
                                int pos = str.LastIndexOf(' ');
                                if (pos != -1)
                                {
                                    ParsingCell newCell = new ParsingCell(cell.left, cell.top);
                                    newCell.addText(new ParsingCellTextFragment(str.Substring(0, pos+1), cell.getFilePosition(0)));
                                    rows[i - 1].Cells[1] = newCell;
                                    rows[i - 1].Cells[2].addText(new ParsingCellTextFragment(str.Substring(pos + 1, str.Length - pos-1), cell.getFilePosition(pos+1)));
                                }
                            }
                        }
                        rows[i - 1].Cells[1].addTexts(row.Cells[1].textFragments);
                        rows.RemoveAt(i);
                        continue;
                    }
                }
                i++;
            }
        }

        bool compareTop(int top1, int top2, int delta)
        {
            if (top1 <= top2 + delta && top1 >= top2 - delta)
                return true;
            return false;
        }

        bool checkForTwoPages(ParsingRow row)
        {
            if (row.Cells[0].left >= 400)
                return true;
            if (row.Cells[0].left + row.Cells[0].text.Length * row.Cells[0].fontSize * 3 / 7 > 400)
                return false;
            if (row.Cells.Count <= 1)
                return true;
            if (row.Cells[1].left >= 400)
                return true;
            return false;
        }

        bool cellsAreIntersected(ParsingCell cell1, ParsingCell cell2)
        {
            if (cell2.left + cell2.getTextLength() < cell1.left)
                return false;
            if (cell1.left + cell1.getTextLength() < cell2.left)
                return false;
            return true;
        }
    }

    class CellInfo
    {
        public ParsingCell firstCell;
        public ParsingCell secondCell;
        public int left;
        public int right;
        public int rowsCount;
        public int fontSize;
        public int filling;
        public int textSize;
        public int maxCellTextSize;
        public int firstRowN;
        public int lastRowN;
        public bool special; // year

        public CellInfo(int l, int r, int fs, int ts, ParsingCell cell)
        {
            left = l;
            right = r;
            rowsCount = 1;
            fontSize = fs;
            filling = 0;
            textSize = ts;
            maxCellTextSize = ts;
            firstCell = cell;
        }
    }

    class RowInfo
    {
        public ParsingCell firstCell;
        public int top;
        public int cellsCount;
        public int fontSize;
    }
}
