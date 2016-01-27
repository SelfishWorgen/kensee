namespace Collector
{
    partial class Options
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxDataFolder = new System.Windows.Forms.TextBox();
            this.buttonBrowseDataFolder = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxDefinitionFile = new System.Windows.Forms.TextBox();
            this.buttonBrowseDefinitionFile = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.numericUpDownLast = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownFirst = new System.Windows.Forms.NumericUpDown();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.checkBoxUpdateEvents = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBoxDate = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxCompany = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxTitle = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxUrl = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.checkBoxCreateExcel = new System.Windows.Forms.CheckBox();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLast)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFirst)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Data folder:";
            // 
            // textBoxDataFolder
            // 
            this.textBoxDataFolder.Enabled = false;
            this.textBoxDataFolder.Location = new System.Drawing.Point(155, 18);
            this.textBoxDataFolder.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBoxDataFolder.Name = "textBoxDataFolder";
            this.textBoxDataFolder.Size = new System.Drawing.Size(528, 22);
            this.textBoxDataFolder.TabIndex = 1;
            // 
            // buttonBrowseDataFolder
            // 
            this.buttonBrowseDataFolder.Location = new System.Drawing.Point(693, 17);
            this.buttonBrowseDataFolder.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonBrowseDataFolder.Name = "buttonBrowseDataFolder";
            this.buttonBrowseDataFolder.Size = new System.Drawing.Size(28, 23);
            this.buttonBrowseDataFolder.TabIndex = 2;
            this.buttonBrowseDataFolder.Text = "...";
            this.buttonBrowseDataFolder.UseVisualStyleBackColor = true;
            this.buttonBrowseDataFolder.Click += new System.EventHandler(this.buttonBrowseDataFolder_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(132, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "Definition file name:";
            // 
            // textBoxDefinitionFile
            // 
            this.textBoxDefinitionFile.Enabled = false;
            this.textBoxDefinitionFile.Location = new System.Drawing.Point(153, 50);
            this.textBoxDefinitionFile.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBoxDefinitionFile.Name = "textBoxDefinitionFile";
            this.textBoxDefinitionFile.Size = new System.Drawing.Size(529, 22);
            this.textBoxDefinitionFile.TabIndex = 4;
            // 
            // buttonBrowseDefinitionFile
            // 
            this.buttonBrowseDefinitionFile.Location = new System.Drawing.Point(693, 50);
            this.buttonBrowseDefinitionFile.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonBrowseDefinitionFile.Name = "buttonBrowseDefinitionFile";
            this.buttonBrowseDefinitionFile.Size = new System.Drawing.Size(28, 23);
            this.buttonBrowseDefinitionFile.TabIndex = 5;
            this.buttonBrowseDefinitionFile.Text = "...";
            this.buttonBrowseDefinitionFile.UseVisualStyleBackColor = true;
            this.buttonBrowseDefinitionFile.Click += new System.EventHandler(this.buttonBrowseDefinitionFile_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.numericUpDownLast);
            this.groupBox2.Controls.Add(this.numericUpDownFirst);
            this.groupBox2.Location = new System.Drawing.Point(19, 84);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox2.Size = new System.Drawing.Size(702, 56);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Collect articles";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(211, 22);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(13, 17);
            this.label8.TabIndex = 7;
            this.label8.Text = "-";
            // 
            // numericUpDownLast
            // 
            this.numericUpDownLast.Location = new System.Drawing.Point(225, 19);
            this.numericUpDownLast.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numericUpDownLast.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownLast.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownLast.Name = "numericUpDownLast";
            this.numericUpDownLast.Size = new System.Drawing.Size(73, 22);
            this.numericUpDownLast.TabIndex = 6;
            this.numericUpDownLast.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // numericUpDownFirst
            // 
            this.numericUpDownFirst.Location = new System.Drawing.Point(147, 19);
            this.numericUpDownFirst.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numericUpDownFirst.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownFirst.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownFirst.Name = "numericUpDownFirst";
            this.numericUpDownFirst.Size = new System.Drawing.Size(61, 22);
            this.numericUpDownFirst.TabIndex = 5;
            this.numericUpDownFirst.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(613, 336);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 26);
            this.buttonCancel.TabIndex = 12;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(517, 336);
            this.buttonSave.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 26);
            this.buttonSave.TabIndex = 11;
            this.buttonSave.Text = "OK";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // checkBoxUpdateEvents
            // 
            this.checkBoxUpdateEvents.AutoSize = true;
            this.checkBoxUpdateEvents.Location = new System.Drawing.Point(20, 333);
            this.checkBoxUpdateEvents.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.checkBoxUpdateEvents.Name = "checkBoxUpdateEvents";
            this.checkBoxUpdateEvents.Size = new System.Drawing.Size(76, 21);
            this.checkBoxUpdateEvents.TabIndex = 14;
            this.checkBoxUpdateEvents.Text = "Update";
            this.checkBoxUpdateEvents.UseVisualStyleBackColor = true;
            this.checkBoxUpdateEvents.Visible = false;
            this.checkBoxUpdateEvents.CheckedChanged += new System.EventHandler(this.checkBoxUpdateEvents_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBoxDate);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.textBoxCompany);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.textBoxTitle);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.textBoxUrl);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(20, 150);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(709, 135);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Process one article for debugging";
            // 
            // textBoxDate
            // 
            this.textBoxDate.Location = new System.Drawing.Point(176, 105);
            this.textBoxDate.Name = "textBoxDate";
            this.textBoxDate.Size = new System.Drawing.Size(190, 22);
            this.textBoxDate.TabIndex = 10;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 108);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(132, 17);
            this.label7.TabIndex = 9;
            this.label7.Text = "Date (yyyy-mm-dd):";
            // 
            // textBoxCompany
            // 
            this.textBoxCompany.Location = new System.Drawing.Point(93, 78);
            this.textBoxCompany.Name = "textBoxCompany";
            this.textBoxCompany.Size = new System.Drawing.Size(602, 22);
            this.textBoxCompany.TabIndex = 8;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 80);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 17);
            this.label6.TabIndex = 7;
            this.label6.Text = "Company:";
            // 
            // textBoxTitle
            // 
            this.textBoxTitle.Location = new System.Drawing.Point(82, 49);
            this.textBoxTitle.Name = "textBoxTitle";
            this.textBoxTitle.Size = new System.Drawing.Size(613, 22);
            this.textBoxTitle.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 51);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 17);
            this.label4.TabIndex = 2;
            this.label4.Text = "Title:";
            // 
            // textBoxUrl
            // 
            this.textBoxUrl.Location = new System.Drawing.Point(82, 22);
            this.textBoxUrl.Name = "textBoxUrl";
            this.textBoxUrl.Size = new System.Drawing.Size(613, 22);
            this.textBoxUrl.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 17);
            this.label3.TabIndex = 0;
            this.label3.Text = "URL:";
            // 
            // checkBoxCreateExcel
            // 
            this.checkBoxCreateExcel.AutoSize = true;
            this.checkBoxCreateExcel.Location = new System.Drawing.Point(20, 301);
            this.checkBoxCreateExcel.Name = "checkBoxCreateExcel";
            this.checkBoxCreateExcel.Size = new System.Drawing.Size(204, 21);
            this.checkBoxCreateExcel.TabIndex = 16;
            this.checkBoxCreateExcel.Text = "Create Excel File For Graph";
            this.checkBoxCreateExcel.UseVisualStyleBackColor = true;
            // 
            // Options
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(741, 371);
            this.Controls.Add(this.checkBoxCreateExcel);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.checkBoxUpdateEvents);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.buttonBrowseDefinitionFile);
            this.Controls.Add(this.textBoxDefinitionFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonBrowseDataFolder);
            this.Controls.Add(this.textBoxDataFolder);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Options";
            this.Text = "Options";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLast)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFirst)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxDataFolder;
        private System.Windows.Forms.Button buttonBrowseDataFolder;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxDefinitionFile;
        private System.Windows.Forms.Button buttonBrowseDefinitionFile;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.CheckBox checkBoxUpdateEvents;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown numericUpDownLast;
        private System.Windows.Forms.NumericUpDown numericUpDownFirst;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxUrl;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxDate;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxCompany;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxTitle;
        private System.Windows.Forms.CheckBox checkBoxCreateExcel;
    }
}