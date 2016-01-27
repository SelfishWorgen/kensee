using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace utl
{
    public class Logger
    {
        string baseFolder;
        string userName;
        System.IO.StreamWriter searchLogFile;

        public Logger(string _baseFolder, string _userName)
        {
            baseFolder = _baseFolder;
            userName = _userName;
        }

        public void close()
        {
            if (searchLogFile != null)
                searchLogFile.Close();
        }

        public void addSearchLog(string str)
        {
            if (searchLogFile == null)
            {
                string n = string.Format("{0:yyyy-MM-dd_hh-mm-ss}.bin", DateTime.Now);
                string nm = "search.log." + userName + "." + n + ".txt";
                string fld = Path.Combine(baseFolder, "log");
                if (!Directory.Exists(fld))
                    Directory.CreateDirectory(fld);
                nm = Path.Combine(fld, nm);
                searchLogFile = new System.IO.StreamWriter(nm, false);
            }
            searchLogFile.WriteLine(str);
        }
    }
}
