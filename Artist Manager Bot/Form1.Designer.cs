namespace Artist_Manager_Bot
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.tokenTextBox = new System.Windows.Forms.TextBox();
            this.textBoxGroupId = new System.Windows.Forms.TextBox();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.richTextBoxMessages = new System.Windows.Forms.RichTextBox();
            this.listBoxDialogs = new System.Windows.Forms.ListBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dataGridUsers = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.checkBoxModuleEnabled = new System.Windows.Forms.CheckBox();
            this.textBoxModuleInfo = new System.Windows.Forms.TextBox();
            this.listBoxModules = new System.Windows.Forms.ListBox();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage5.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridUsers)).BeginInit();
            this.tabPage1.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // tokenTextBox
            // 
            this.tokenTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tokenTextBox.Location = new System.Drawing.Point(12, 12);
            this.tokenTextBox.Name = "tokenTextBox";
            this.tokenTextBox.Size = new System.Drawing.Size(251, 20);
            this.tokenTextBox.TabIndex = 1;
            this.tokenTextBox.Text = "Token";
            this.tokenTextBox.Enter += new System.EventHandler(this.TokenAndGidTextBox_Enter);
            this.tokenTextBox.Leave += new System.EventHandler(this.TokenTextBox_Leave);
            // 
            // textBoxGroupId
            // 
            this.textBoxGroupId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxGroupId.Location = new System.Drawing.Point(269, 12);
            this.textBoxGroupId.Name = "textBoxGroupId";
            this.textBoxGroupId.Size = new System.Drawing.Size(138, 20);
            this.textBoxGroupId.TabIndex = 6;
            this.textBoxGroupId.Text = "Group ID";
            this.textBoxGroupId.Enter += new System.EventHandler(this.TokenAndGidTextBox_Enter);
            // 
            // buttonStart
            // 
            this.buttonStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStart.Location = new System.Drawing.Point(414, 12);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(57, 20);
            this.buttonStart.TabIndex = 1;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.ButtonStart_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStop.Enabled = false;
            this.buttonStop.Location = new System.Drawing.Point(477, 12);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(57, 20);
            this.buttonStop.TabIndex = 7;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.ButtonStop_Click);
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.textBoxLog);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Size = new System.Drawing.Size(515, 224);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Log";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // textBoxLog
            // 
            this.textBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxLog.Location = new System.Drawing.Point(0, 0);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.Size = new System.Drawing.Size(515, 224);
            this.textBoxLog.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.richTextBoxMessages);
            this.tabPage3.Controls.Add(this.listBoxDialogs);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(515, 224);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Messages";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            this.richTextBoxMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxMessages.Location = new System.Drawing.Point(126, 3);
            this.richTextBoxMessages.Name = "richTextBox1";
            this.richTextBoxMessages.Size = new System.Drawing.Size(386, 218);
            this.richTextBoxMessages.TabIndex = 1;
            this.richTextBoxMessages.Text = "";
            // 
            // listBoxDialogs
            // 
            this.listBoxDialogs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxDialogs.FormattingEnabled = true;
            this.listBoxDialogs.IntegralHeight = false;
            this.listBoxDialogs.Items.AddRange(new object[] {
            "All"});
            this.listBoxDialogs.Location = new System.Drawing.Point(3, 3);
            this.listBoxDialogs.Name = "listBoxDialogs";
            this.listBoxDialogs.Size = new System.Drawing.Size(117, 218);
            this.listBoxDialogs.TabIndex = 0;
            this.listBoxDialogs.SelectedIndexChanged += new System.EventHandler(this.ListBoxDialogs_SelectedIndexChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dataGridUsers);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(515, 224);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Users";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dataGridUsers
            // 
            this.dataGridUsers.AllowUserToAddRows = false;
            this.dataGridUsers.AllowUserToDeleteRows = false;
            this.dataGridUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridUsers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridUsers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2});
            this.dataGridUsers.Location = new System.Drawing.Point(6, 6);
            this.dataGridUsers.Name = "dataGridUsers";
            this.dataGridUsers.ReadOnly = true;
            this.dataGridUsers.RowHeadersVisible = false;
            this.dataGridUsers.Size = new System.Drawing.Size(503, 212);
            this.dataGridUsers.TabIndex = 0;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "UserID";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "Name";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.checkBoxModuleEnabled);
            this.tabPage1.Controls.Add(this.textBoxModuleInfo);
            this.tabPage1.Controls.Add(this.listBoxModules);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(515, 224);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Modules";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // checkBoxModuleEnabled
            // 
            this.checkBoxModuleEnabled.AutoSize = true;
            this.checkBoxModuleEnabled.Enabled = false;
            this.checkBoxModuleEnabled.Location = new System.Drawing.Point(132, 201);
            this.checkBoxModuleEnabled.Name = "checkBoxModuleEnabled";
            this.checkBoxModuleEnabled.Size = new System.Drawing.Size(65, 17);
            this.checkBoxModuleEnabled.TabIndex = 5;
            this.checkBoxModuleEnabled.Text = "Enabled";
            this.checkBoxModuleEnabled.UseVisualStyleBackColor = true;
            this.checkBoxModuleEnabled.CheckedChanged += new System.EventHandler(this.CheckBoxModuleEnabled_CheckedChanged);
            // 
            // textBoxModuleInfo
            // 
            this.textBoxModuleInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxModuleInfo.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.textBoxModuleInfo.Location = new System.Drawing.Point(132, 6);
            this.textBoxModuleInfo.Multiline = true;
            this.textBoxModuleInfo.Name = "textBoxModuleInfo";
            this.textBoxModuleInfo.ReadOnly = true;
            this.textBoxModuleInfo.Size = new System.Drawing.Size(377, 189);
            this.textBoxModuleInfo.TabIndex = 4;
            // 
            // listBoxModules
            // 
            this.listBoxModules.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxModules.FormattingEnabled = true;
            this.listBoxModules.IntegralHeight = false;
            this.listBoxModules.Location = new System.Drawing.Point(6, 6);
            this.listBoxModules.Name = "listBoxModules";
            this.listBoxModules.Size = new System.Drawing.Size(120, 212);
            this.listBoxModules.TabIndex = 3;
            this.listBoxModules.SelectedIndexChanged += new System.EventHandler(this.ListBoxModules_SelectedIndexChanged);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Controls.Add(this.tabPage3);
            this.tabControl.Controls.Add(this.tabPage5);
            this.tabControl.HotTrack = true;
            this.tabControl.Location = new System.Drawing.Point(12, 38);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(523, 250);
            this.tabControl.TabIndex = 5;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(547, 295);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.textBoxGroupId);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.tokenTextBox);
            this.Name = "Form1";
            this.Text = "Artist Manager Bot";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabPage5.ResumeLayout(false);
            this.tabPage5.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridUsers)).EndInit();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox tokenTextBox;
        private System.Windows.Forms.TextBox textBoxGroupId;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.RichTextBox richTextBoxMessages;
        private System.Windows.Forms.ListBox listBoxDialogs;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView dataGridUsers;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.CheckBox checkBoxModuleEnabled;
        private System.Windows.Forms.TextBox textBoxModuleInfo;
        private System.Windows.Forms.ListBox listBoxModules;
        private System.Windows.Forms.TabControl tabControl;
    }
}

