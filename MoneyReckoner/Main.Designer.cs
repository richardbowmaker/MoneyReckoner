
namespace MoneyReckoner
{
    partial class Main
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
            this.chkSantanderCurrent = new System.Windows.Forms.CheckBox();
            this.chkCashPlus = new System.Windows.Forms.CheckBox();
            this.chkSantanderCredit = new System.Windows.Forms.CheckBox();
            this.cmdSummaries = new System.Windows.Forms.Button();
            this.cmdSave = new System.Windows.Forms.Button();
            this.cmdLoad = new System.Windows.Forms.Button();
            this.cmdClearStatement = new System.Windows.Forms.Button();
            this.cmdClearLog = new System.Windows.Forms.Button();
            this.lstLogger = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // chkSantanderCurrent
            // 
            this.chkSantanderCurrent.AutoSize = true;
            this.chkSantanderCurrent.Location = new System.Drawing.Point(43, 40);
            this.chkSantanderCurrent.Name = "chkSantanderCurrent";
            this.chkSantanderCurrent.Size = new System.Drawing.Size(167, 24);
            this.chkSantanderCurrent.TabIndex = 0;
            this.chkSantanderCurrent.Text = "Santander Current";
            this.chkSantanderCurrent.UseVisualStyleBackColor = true;
            // 
            // chkCashPlus
            // 
            this.chkCashPlus.AutoSize = true;
            this.chkCashPlus.Location = new System.Drawing.Point(43, 100);
            this.chkCashPlus.Name = "chkCashPlus";
            this.chkCashPlus.Size = new System.Drawing.Size(106, 24);
            this.chkCashPlus.TabIndex = 1;
            this.chkCashPlus.Text = "Cash Plus";
            this.chkCashPlus.UseVisualStyleBackColor = true;
            // 
            // chkSantanderCredit
            // 
            this.chkSantanderCredit.AutoSize = true;
            this.chkSantanderCredit.Location = new System.Drawing.Point(43, 70);
            this.chkSantanderCredit.Name = "chkSantanderCredit";
            this.chkSantanderCredit.Size = new System.Drawing.Size(194, 24);
            this.chkSantanderCredit.TabIndex = 3;
            this.chkSantanderCredit.Text = "Santander Credit Card";
            this.chkSantanderCredit.UseVisualStyleBackColor = true;
            // 
            // cmdSummaries
            // 
            this.cmdSummaries.Location = new System.Drawing.Point(351, 40);
            this.cmdSummaries.Name = "cmdSummaries";
            this.cmdSummaries.Size = new System.Drawing.Size(247, 54);
            this.cmdSummaries.TabIndex = 4;
            this.cmdSummaries.Text = "Show summaries";
            this.cmdSummaries.UseVisualStyleBackColor = true;
            this.cmdSummaries.Click += new System.EventHandler(this.cmdSummaries_Click);
            // 
            // cmdSave
            // 
            this.cmdSave.Location = new System.Drawing.Point(757, 51);
            this.cmdSave.Name = "cmdSave";
            this.cmdSave.Size = new System.Drawing.Size(103, 42);
            this.cmdSave.TabIndex = 5;
            this.cmdSave.Text = "Save";
            this.cmdSave.UseVisualStyleBackColor = true;
            this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
            // 
            // cmdLoad
            // 
            this.cmdLoad.Location = new System.Drawing.Point(757, 128);
            this.cmdLoad.Name = "cmdLoad";
            this.cmdLoad.Size = new System.Drawing.Size(102, 44);
            this.cmdLoad.TabIndex = 6;
            this.cmdLoad.Text = "Load";
            this.cmdLoad.UseVisualStyleBackColor = true;
            this.cmdLoad.Click += new System.EventHandler(this.cmdLoad_Click);
            // 
            // cmdClearStatement
            // 
            this.cmdClearStatement.Location = new System.Drawing.Point(924, 53);
            this.cmdClearStatement.Name = "cmdClearStatement";
            this.cmdClearStatement.Size = new System.Drawing.Size(149, 40);
            this.cmdClearStatement.TabIndex = 7;
            this.cmdClearStatement.Text = "Clear Statement";
            this.cmdClearStatement.UseVisualStyleBackColor = true;
            this.cmdClearStatement.Click += new System.EventHandler(this.cmdClear_Click);
            // 
            // cmdClearLog
            // 
            this.cmdClearLog.Location = new System.Drawing.Point(349, 150);
            this.cmdClearLog.Name = "cmdClearLog";
            this.cmdClearLog.Size = new System.Drawing.Size(149, 51);
            this.cmdClearLog.TabIndex = 8;
            this.cmdClearLog.Text = "Clear Log";
            this.cmdClearLog.UseVisualStyleBackColor = true;
            this.cmdClearLog.Click += new System.EventHandler(this.cmdClearLog_Click);
            // 
            // lstLogger
            // 
            this.lstLogger.FormattingEnabled = true;
            this.lstLogger.HorizontalScrollbar = true;
            this.lstLogger.ItemHeight = 20;
            this.lstLogger.Location = new System.Drawing.Point(43, 281);
            this.lstLogger.Name = "lstLogger";
            this.lstLogger.Size = new System.Drawing.Size(1494, 784);
            this.lstLogger.TabIndex = 9;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1860, 1134);
            this.Controls.Add(this.lstLogger);
            this.Controls.Add(this.cmdClearLog);
            this.Controls.Add(this.cmdClearStatement);
            this.Controls.Add(this.cmdLoad);
            this.Controls.Add(this.cmdSave);
            this.Controls.Add(this.cmdSummaries);
            this.Controls.Add(this.chkSantanderCredit);
            this.Controls.Add(this.chkCashPlus);
            this.Controls.Add(this.chkSantanderCurrent);
            this.Name = "Main";
            this.Text = "Money Reckoner";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkSantanderCurrent;
        private System.Windows.Forms.CheckBox chkCashPlus;
        private System.Windows.Forms.CheckBox chkSantanderCredit;
        private System.Windows.Forms.Button cmdSummaries;
        private System.Windows.Forms.Button cmdSave;
        private System.Windows.Forms.Button cmdLoad;
        private System.Windows.Forms.Button cmdClearStatement;
        private System.Windows.Forms.Button cmdClearLog;
        private System.Windows.Forms.ListBox lstLogger;
    }
}

