using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using ReEvents;

namespace Collector
{
    public partial class Options : Form
    {
        public bool updateSentiments;
        public bool updateEvents;
        public bool processSeparateUrl;
        public bool isOK;

        public int firstNumber;
        public int lastNumber;

        ReEventOptions opt;
        FolderBrowserDialog openDataFolderDlg;
        OpenFileDialog openDefinitionFileDlg;

        public string separateURL;
        public string separateTitle;
        public string separateDateStr;
        public string separateCompany;
        public bool createExcelFile;
        
        public Options(ReEventOptions options)
        {

            InitializeComponent();

            isOK = false;

            opt = options;

            textBoxDataFolder.Text = opt.FilesPath;
            textBoxDefinitionFile.Text = opt.definitionsFileName;

            numericUpDownFirst.Value = 1;
            numericUpDownLast.Value = 100;

            processSeparateUrl = false;
            separateURL = "";
            separateTitle = "";
            separateDateStr = "";
            separateCompany = "";
            createExcelFile = false;
            checkBoxCreateExcel.Checked = false;
        }

        private void buttonBrowseDataFolder_Click(object sender, EventArgs e)
        {
            if (openDataFolderDlg == null)
            {
                openDataFolderDlg = new FolderBrowserDialog();
            }
            openDataFolderDlg.SelectedPath = textBoxDataFolder.Text;
            openDataFolderDlg.ShowDialog();
            if (!string.IsNullOrEmpty(openDataFolderDlg.SelectedPath))
                textBoxDataFolder.Text = openDataFolderDlg.SelectedPath;
        }

        private void buttonBrowseDefinitionFile_Click(object sender, EventArgs e)
        {
            if (openDefinitionFileDlg == null)
            {
                openDefinitionFileDlg = new OpenFileDialog();
                if (openDefinitionFileDlg.InitialDirectory == null)
                    openDefinitionFileDlg.InitialDirectory = opt.FilesPath;
                if (!string.IsNullOrWhiteSpace(textBoxDefinitionFile.Text))
                    openDefinitionFileDlg.InitialDirectory = new FileInfo(textBoxDefinitionFile.Text).Directory.FullName;
                openDefinitionFileDlg.FileOk += openDefinitionFileDlg_FileOk;
            }
            openDefinitionFileDlg.ShowDialog();
        }

        void openDefinitionFileDlg_FileOk(object sender, CancelEventArgs e)
        {
            textBoxDefinitionFile.Text = openDefinitionFileDlg.FileName;
        }


        #region Validation
        private bool isValidFileInTextBox(TextBox textBox, string name)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                MessageBox.Show(name + " is empty");
                textBox.Focus();
                return false;
            }
            FileInfo f = new FileInfo(textBox.Text.Trim());
            if (!f.Exists)
            {
                MessageBox.Show(name + " does not exists");
                textBox.Focus();
                return false;
            }
            return true;
        }

        private bool isValidDirectoryInTextBox(TextBox textBox, string name)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                MessageBox.Show(name + " is empty");
                textBox.Focus();
                return false;
            }
            DirectoryInfo d = new DirectoryInfo(textBox.Text.Trim());
            if (!d.Exists)
            {
                MessageBox.Show(name + " does not exists");
                textBox.Focus();
                return false;
            }
            return true;
        }

        private bool isValidTextInTextBox(TextBox textBox, string name)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                MessageBox.Show(name + " is empty");
                textBox.Focus();
                return false;
            }
            return true;
        }
        #endregion
        
        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (!isValidDirectoryInTextBox(textBoxDataFolder, "Data folder name"))
                return;
            if (!isValidFileInTextBox(textBoxDefinitionFile, "Definition file name"))
                return;

            //to check files exists in data folder
            opt.FilesPath = textBoxDataFolder.Text;
            opt.definitionsFileName = textBoxDefinitionFile.Text;

            opt.SaveOptionsToIniFile();
            isOK = true;
            updateEvents = checkBoxUpdateEvents.Checked;
            updateSentiments = checkBoxUpdateEvents.Checked;
            firstNumber = updateEvents ? 1 : (int) numericUpDownFirst.Value;
            lastNumber = updateEvents ? 100 : (int) numericUpDownLast.Value;

            processSeparateUrl = !String.IsNullOrEmpty(textBoxUrl.Text);
            separateURL = textBoxUrl.Text;
            separateTitle = textBoxTitle.Text;
            separateDateStr = textBoxDate.Text;
            separateCompany = textBoxCompany.Text;
            createExcelFile = checkBoxCreateExcel.Checked;
            Close();
        }

        private void checkBoxUpdateEvents_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownFirst.Enabled = !checkBoxUpdateEvents.Checked;
            numericUpDownLast.Enabled = !checkBoxUpdateEvents.Checked;
        }

    }
}
