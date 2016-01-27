using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;

namespace ExcelWriter
{
    public class ExcelWriter
    {
        SpreadsheetDocument package;
        WorkbookPart workbookPart;
        Sheets sheets;
        uint sheetId;
        Row currentRow;
        SheetData sheetData;

        public ExcelWriter(string fileName)
        {
            package = DocumentFormat.OpenXml.Packaging.SpreadsheetDocument.Create(fileName, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
            package.CompressionOption = System.IO.Packaging.CompressionOption.Normal;
            workbookPart = package.AddWorkbookPart();

            SharedStringTablePart sharedStringTablePart;
            sharedStringTablePart = package.WorkbookPart.AddNewPart<SharedStringTablePart>();
            sharedStringTablePart.SharedStringTable = new SharedStringTable();
            sharedStringTablePart.SharedStringTable.Save();

            var workbook1 = new DocumentFormat.OpenXml.Spreadsheet.Workbook();
            workbook1.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
            sheets = new DocumentFormat.OpenXml.Spreadsheet.Sheets();
            workbook1.Append(sheets);
            workbookPart.Workbook = workbook1;
            WorkbookStylesPart wbsp = workbookPart.AddNewPart<WorkbookStylesPart>();
            wbsp.Stylesheet = CreateStylesheet();
            wbsp.Stylesheet.Save();
            sheetId = 0;
        }

        public void Save()
        {
            package.Close();
        }

        public void addEmptyRow()
        {
            NewRow();
            AddCellToCurrentRow(" ");
            addRow();
        }

        public void addStringRow(string str, string str1)
        {
            NewRow();
            AddCellToCurrentRow(str);
            AddCellToCurrentRow(str1);
            addRow();
        }

        public DocumentFormat.OpenXml.Spreadsheet.SheetData newSheet(string name, List<int> columnsizes)
        {
            sheetId++;
            var currentSheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet()
            {
                Name = name,
                SheetId = (UInt32Value)sheetId,
                Id = "rId" + sheetId
            };
            sheets.Append(currentSheet);
            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>("rId" + sheetId);
            var worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet();
            sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();

            Columns columns = new Columns();
            uint k = 1;
            foreach (var x in columnsizes)
                columns.Append(CreateColumnData(k++, k, x));
            worksheet.Append(columns);
            worksheet.Append(sheetData);
            worksheetPart.Worksheet = worksheet;
            return sheetData;
        }

        public DocumentFormat.OpenXml.Spreadsheet.Row NewRow()
        {
            currentRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
            return currentRow;
        }

        public void AddCellToCurrentRow(string name, uint id = 0)
        {
            if (name == null)
                name = "";
            //if (name.IndexOf("&") != -1 && name.IndexOf(";") != -1)
            //{
            //    System.IO.StreamWriter file = new System.IO.StreamWriter("c:\\val\\a.txt", true);
            //    file.WriteLine(name);
            //    file.WriteLine("\n");
            //    file.Close();
            //}
            Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
            cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(name);
            cell.StyleIndex = id;
            currentRow.AppendChild(cell);
        }

        public void AddIntCellToCurrentRow(string value, uint id = 0)
        {
            if (value == null)
                value = "";
            Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
            if (string.IsNullOrWhiteSpace(value))
            {
                cell.StyleIndex = id;
                cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
            }
            else
            {
                //try
                //{
                //    var xx = Convert.ToDouble(value);
                //    cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.Number;
                //    if (value.IndexOf('.') == -1 && value.IndexOf(',') == -1)
                //        cell.StyleIndex = 4 + id;
                //    else
                //    {
                //        value = value.Replace(",", ".");
                //        if (value == xx.ToString("0.##").Replace(",", "."))
                //        {
                //            cell.StyleIndex = 2 + id;
                //        }
                //        else
                //        {
                //            cell.StyleIndex = id;
                //            cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                    cell.StyleIndex = id;
                    cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //        }
            }
            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue((value));
            currentRow.AppendChild(cell);
        }

        public void addRow()
        {
            sheetData.Append(currentRow);
        }

        private static Column CreateColumnData(UInt32 StartColumnIndex, UInt32 EndColumnIndex, double ColumnWidth)
        {
            Column column;
            column = new Column();
            column.Min = StartColumnIndex;
            column.Max = EndColumnIndex;
            column.Width = ColumnWidth;
            column.CustomWidth = true;
            return column;
        }

        private  Stylesheet CreateStylesheet()
        {
            Stylesheet ss = new Stylesheet();

            Fonts fts = new Fonts();
            DocumentFormat.OpenXml.Spreadsheet.Font ft = new DocumentFormat.OpenXml.Spreadsheet.Font();
            FontName ftn = new FontName();
            ftn.Val = "Calibri";
            FontSize ftsz = new FontSize();
            ftsz.Val = 11;
            ft.FontName = ftn;
            ft.FontSize = ftsz;
            fts.Append(ft);
            fts.Count = (uint)fts.ChildElements.Count;

            ft = new DocumentFormat.OpenXml.Spreadsheet.Font();
            ftn = new FontName();
            ftn.Val = "Calibri";
            ftsz = new FontSize();
            ftsz.Val = 11;
            ft.FontName = ftn;
            ft.FontSize = ftsz;
            ft.Bold = new Bold();
            fts.Append(ft);
            fts.Count = (uint)fts.ChildElements.Count;

            Fills fills = new Fills();
            Fill fill;
            PatternFill patternFill;
            fill = new Fill();
            patternFill = new PatternFill();
            patternFill.PatternType = PatternValues.None;
            fill.PatternFill = patternFill;
            fills.Append(fill);
            fill = new Fill();
            patternFill = new PatternFill();
            patternFill.PatternType = PatternValues.Gray125;
            fill.PatternFill = patternFill;
            fills.Append(fill);
            fills.Count = (uint)fills.ChildElements.Count;

            Borders borders = new Borders();
            Border border = new Border();
            border.LeftBorder = new LeftBorder();
            border.RightBorder = new RightBorder();
            border.TopBorder = new TopBorder();
            border.BottomBorder = new BottomBorder();
            border.DiagonalBorder = new DiagonalBorder();
            borders.Append(border);
            borders.Count = (uint)borders.ChildElements.Count;

            CellStyleFormats csfs = new CellStyleFormats();
            CellFormat cf = new CellFormat();
            cf.NumberFormatId = 0;
            cf.FontId = 0;
            cf.FillId = 0;
            cf.BorderId = 0;
            csfs.Append(cf);

            cf = new CellFormat();
            cf.NumberFormatId = 0;
            cf.FontId = 1;
            cf.FillId = 0;
            cf.BorderId = 0;
            csfs.Append(cf);

            cf = new CellFormat(); // numeric
            cf.NumberFormatId = 4;
            cf.FontId = 0;
            cf.FillId = 0;
            cf.BorderId = 0;
            cf.ApplyNumberFormat = true;
            csfs.Append(cf);

            cf = new CellFormat(); // numeric bold
            cf.NumberFormatId = 4;
            cf.FontId = 1;
            cf.FillId = 0;
            cf.BorderId = 0;
            cf.ApplyNumberFormat = true;
            csfs.Append(cf);

            cf = new CellFormat(); // numeric
            cf.NumberFormatId = 3;
            cf.FontId = 0;
            cf.FillId = 0;
            cf.BorderId = 0;
            cf.ApplyNumberFormat = true;
            csfs.Append(cf);

            cf = new CellFormat(); // numeric bold
            cf.NumberFormatId = 3;
            cf.FontId = 1;
            cf.FillId = 0;
            cf.BorderId = 0;
            cf.ApplyNumberFormat = true;
            csfs.Append(cf);

            csfs.Count = (uint)csfs.ChildElements.Count;

            CellFormats cfs = new CellFormats();

            cf = new CellFormat();
            cf.NumberFormatId = 0;
            cf.FontId = 0;
            cf.FillId = 0;
            cf.BorderId = 0;
            cf.FormatId = 0;
            cf.ApplyAlignment = true;
            Alignment alignment1 = new Alignment() { WrapText = true, Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Center };
            cf.Append(alignment1);
            cfs.Append(cf);

            cf = new CellFormat();
            cf.NumberFormatId = 0;
            cf.FontId = 1;
            cf.FillId = 0;
            cf.BorderId = 0;
            cf.FormatId = 1;
            cf.ApplyAlignment = true;
            Alignment alignment2 = new Alignment() { WrapText = true, Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Center };
            cf.Append(alignment2);
            cfs.Append(cf);

            cf = new CellFormat(); // numeric
            cf.NumberFormatId = 4;
            cf.FontId = 0;
            cf.FillId = 0;
            cf.BorderId = 0;
            cf.ApplyNumberFormat = true;
            cf.FormatId = 2;
            cf.ApplyAlignment = true;
            cf.Append(new Alignment() { WrapText = true, Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Center });
            cfs.Append(cf);

            cf = new CellFormat(); // numeric
            cf.NumberFormatId = 4;
            cf.FontId = 1;
            cf.FillId = 0;
            cf.BorderId = 0;
            cf.ApplyNumberFormat = true;
            cf.FormatId = 3;
            cf.ApplyAlignment = true;
            cf.Append(new Alignment() { WrapText = true, Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Center });
            cfs.Append(cf);

            cf = new CellFormat(); // numeric
            cf.NumberFormatId = 3;
            cf.FontId = 0;
            cf.FillId = 0;
            cf.BorderId = 0;
            cf.ApplyNumberFormat = true;
            cf.FormatId = 4;
            cf.ApplyAlignment = true;
            cf.Append(new Alignment() { WrapText = true, Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Center });
            cfs.Append(cf);

            cf = new CellFormat(); // numeric
            cf.NumberFormatId = 3;
            cf.FontId = 1;
            cf.FillId = 0;
            cf.BorderId = 0;
            cf.ApplyNumberFormat = true;
            cf.FormatId = 5;
            cf.ApplyAlignment = true;
            cf.Append(new Alignment() { WrapText = true, Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Center });
            cfs.Append(cf);

            cfs.Count = (uint)cfs.ChildElements.Count;

            ss.Append(fts);
            ss.Append(fills);
            ss.Append(borders);
            ss.Append(csfs);
            ss.Append(cfs);

            CellStyles css = new CellStyles();
            CellStyle cs = new CellStyle();                      // normal 0
            cs.Name = "Normal";
            cs.FormatId = 0;
            cs.BuiltinId = 0;
            css.Append(cs);
            css.Count = (uint)css.ChildElements.Count;

            cs = new CellStyle();                                  // bold 1
            cs.Name = "Bold";
            cs.FormatId = 1;
            cs.BuiltinId = 0;
            css.Append(cs);
            css.Count = (uint)css.ChildElements.Count;

            cs = new CellStyle();                                   // numeric normal 2
            cs.Name = "NormalNum";
            cs.FormatId = 2;
            cs.BuiltinId = 0;
            css.Append(cs);
            css.Count = (uint)css.ChildElements.Count;

            cs = new CellStyle();                                   // numeric bold 3
            cs.Name = "BoldNum";
            cs.FormatId = 3;
            cs.BuiltinId = 0;
            css.Append(cs);
            css.Count = (uint)css.ChildElements.Count;

            cs = new CellStyle();                                   // numeric normal 2
            cs.Name = "NormalNum";
            cs.FormatId = 4;
            cs.BuiltinId = 0;
            css.Append(cs);
            css.Count = (uint)css.ChildElements.Count;

            cs = new CellStyle();                                   // numeric bold 3
            cs.Name = "BoldNum";
            cs.FormatId = 5;
            cs.BuiltinId = 0;
            css.Append(cs);
            css.Count = (uint)css.ChildElements.Count;

            ss.Append(css);

            DifferentialFormats dfs = new DifferentialFormats();
            dfs.Count = 0;
            ss.Append(dfs);

            TableStyles tss = new TableStyles();
            tss.Count = 0;
            tss.DefaultTableStyle = "TableStyleMedium9";
            tss.DefaultPivotStyle = "PivotStyleLight16";
            ss.Append(tss);

            return ss;
        }
    }
}
