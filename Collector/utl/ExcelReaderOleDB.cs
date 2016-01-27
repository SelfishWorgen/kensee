using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.IO;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Data;
using System.Windows.Forms;

namespace utl
{
    public class ExcelReaderOleDB
    {
        DataSet dataSet = null;
        string fileName;
        string[] sheetNames;

        public int SheetCount { get { return sheetNames == null ? 0 : sheetNames.Length; } }

        public ExcelReaderOleDB(string fileNm)
        {
            try
            {
                fileName = fileNm;
                string ext = Path.GetExtension(fileNm).ToLower();
                switch (ext)
                {
                    case ".xlsx":
                        sheetNames = GetExcelSheetNames();
                        dataSet = ImportExcelXLS(fileNm, false, false);
                        break;
                    default:
                        System.Console.WriteLine("Reading from " + ext + " file is not implemented yet.");
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception: " + ex.Message);
            }
        }

        int getPosition(HtmlNode node)
        {
            if (node.NodeType == HtmlNodeType.Text)
                return node.StreamPosition;
            foreach (var x in node.ChildNodes)
            {
                int p = getPosition(x);
                if (p != 1)
                    return p;
            }
            return 1;
        }

        int getIntAttribute(HtmlNode node, string name)
        {
            int value = 0;
            for (int i = 0; i < node.Attributes.Count; i++)
            {
                if (node.Attributes[i].Name == name)
                {
                    Int32.TryParse(node.Attributes[i].Value, out value);
                    break;
                }
            }
            return value;
        }

        string replaceChars(string str)
        {
            if (str == null)
                return "";
            return str.Replace("\t", " ").Replace("&nbsp;", " ").Replace("&raquo;", " ").Replace("&yen", "¥").Trim();
        }
        
        String[] GetExcelSheetNames()
        {
            OleDbConnection objConn = null;
            System.Data.DataTable dt = null;
            try
            {
                string connString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=    ""Excel 12.0;HDR=YES;""";
                connString = string.Format(connString, fileName);
                objConn = new OleDbConnection(connString);
                objConn.Open();
                dt = objConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                if (dt == null)
                {
                    System.Console.WriteLine("GetExcelSheetNames: " + "no OleDbSchemaTable");
                    return null;
                }
                String[] excelSheets = new String[dt.Rows.Count];
                int i = 0;
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    excelSheets[i] = row["TABLE_NAME"].ToString();
                    i++;
                }
                return excelSheets;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception: " + ex.Message);
                return null;
            }
            finally
            {
                if (objConn != null)
                {
                    objConn.Close();
                    objConn.Dispose();
                }
                if (dt != null)
                    dt.Dispose();
            }
        }

        public List<ParsingRow> GetRows(int sheetN)
        {
            HtmlParser.excelParsed = true;
            var rows = new List<ParsingRow>();
            if (dataSet != null && sheetN < dataSet.Tables.Count)
            {
                for (int ir = 0; ir < dataSet.Tables[sheetN].Rows.Count; ir++)
                {
                    var x = dataSet.Tables[sheetN].Rows[ir];
                    ParsingRow newRow = new ParsingRow();
                    int i = x.ItemArray.Length - 1;
                    for (; i >= 0; i--)
                    {
                        if (x.ItemArray[i].ToString() == "")
                            continue;
                        else
                            break;
                    }
                    for (int count = 0; count <= i; count++)
                    {
                        ParsingCell cell = new ParsingCell(0, 0);
                        var tf = new ParsingCellTextFragment(x.ItemArray[count].ToString(), ir);
                        tf.cellNumber = count;
                        cell.addText(tf);
                        newRow.AddCell(cell);
                    }
                    rows.Add(newRow);
                }
            }
            return rows;
        }

        public static DataSet ImportExcelXLS(string FileName, bool hasHeaders, bool fromUI)
        {
            string HDR =   "Yes"  ;
            string strConn;
            strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FileName + ";Extended Properties=\"Excel 12.0;ReadOnly=true;HDR=" + HDR + ";IMEX=1;\"";

            DataSet output = new DataSet();

            try
            {
                using (OleDbConnection conn = new OleDbConnection(strConn))
                {
                    conn.Open();
                    DataTable schemaTable = conn.GetOleDbSchemaTable(
                        OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

                    foreach (DataRow schemaRow in schemaTable.Rows)
                    {
                        string sheet = schemaRow["TABLE_NAME"].ToString();

                        if (!sheet.EndsWith("_"))
                        {
                            try
                            {
                                OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + sheet + "]", conn);
                                cmd.CommandType = CommandType.Text;

                                DataTable outputTable = new DataTable(sheet);
                                output.Tables.Add(outputTable);
                                new OleDbDataAdapter(cmd).Fill(outputTable);
                            }
                            catch (Exception ex)
                            {
                                break;
                                //              throw new Exception(ex.Message + string.Format("Sheet:{0}.File:F{1}", sheet, FileName), ex);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (fromUI)
                    MessageBox.Show("Exception: " + ex.Message + "\nCan’t access the definition file.\nPlease check if the file name is correct and if the file is not opened in Excel.");
                else
                    System.Console.WriteLine("Exception: " + ex.Message + "\nCan’t access the file " + FileName + ".\nPlease check if the file name is correct and if the file is not opened in Excel.");
            }
            return output;
        }

        public static void ExportExcelXLS(string FileName, bool hasHeaders, DataSet dataSet)
        {
            string HDR =  "Yes" ;
            string strConn;
            if (FileName.Substring(FileName.LastIndexOf('.')).ToLower() == ".xlsx")
                strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FileName + ";Extended Properties=\"Excel 12.0;HDR=" + HDR + ";IMEX=1\"";
            else
                strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + FileName + ";Extended Properties=\"Excel 8.0;HDR=" + HDR + ";IMEX=1\"";

            var x = dataSet.GetChanges();
            int i = 0;
            using (OleDbConnection conn = new OleDbConnection(strConn))
            {
                conn.Open();
                DataTable schemaTable = conn.GetOleDbSchemaTable(
                    OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

                foreach (DataRow schemaRow in schemaTable.Rows)
                {
                    string sheet = schemaRow["TABLE_NAME"].ToString();

                    if (!sheet.EndsWith("_"))
                    {
                        if (dataSet != null && dataSet.Tables[i] != null && dataSet.HasChanges(DataRowState.Modified))
                        {
                            try
                            {
                                OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + sheet + "]", conn);
                                cmd.CommandType = CommandType.Text;
                                DataTable dt = dataSet.Tables[i].GetChanges(DataRowState.Modified);
                                if (dt != null)
                                {
                                    var ada = new OleDbDataAdapter(cmd);
                                    OleDbCommandBuilder cb = new OleDbCommandBuilder(ada);
                                    ada.UpdateCommand = cb.GetUpdateCommand();
                                    ada.DeleteCommand = cb.GetDeleteCommand();
                                    ada.InsertCommand = cb.GetInsertCommand();

                                    ada.Update(dt); //Error occurs here: "No value given for required parameters"
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message + string.Format("Sheet:{0}.File:F{1}", sheet, FileName), ex);
                            }
                        }
                        i++;
                    }
                }
            }
        }
    }
}
