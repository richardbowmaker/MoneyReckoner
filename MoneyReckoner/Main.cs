using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

// save previous statements
// add in cash plus
// improve displayed output

namespace MoneyReckoner
{

    public partial class Main : Form
    {
        public static Main This;
        private CaptureClipboard _captureClipboard;

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            This = this;
            _captureClipboard = new CaptureClipboard();
            _captureClipboard.Initialise(this);

            chkCashPlus.Enabled = false;
            chkCashPlus.Checked = false;
            chkSantanderCurrent.Enabled = false;
            chkSantanderCurrent.Checked = false;
            chkSantanderCredit.Enabled = false;
            chkSantanderCredit.Checked = false;
        }

        public void SetCashplus()
        {
            chkCashPlus.Checked = true;
            Log("Cash Plus statement");
        }

        public void SetSantanderCurrent()
        {
            chkSantanderCurrent.Checked = true;
            Log("Santander current account statement");
        }
        public void SetSantanderCredit()
        {
            chkSantanderCredit.Checked = true;
            Log("Santander credit card statement");
        }

        public void Log(string log)
        {
            txtLog.Text += log + Environment.NewLine;
        }

        private void cmdWeeklySummaries_Click(object sender, EventArgs e)
        {
            Data.GenerateWeeklySummaries();
            Data.SummariesToLog();
        }

        private void cmdSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Text File|*.txt";
            dlg.Title = "Save statement";
            
            if (dlg.ShowDialog() == DialogResult.OK)
                Data.Serialise(dlg.FileName, false);
        }

        private void cmdLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Text File|*.txt";
            dlg.Title = "Save statement";

            if (dlg.ShowDialog() == DialogResult.OK)
                Data.Serialise(dlg.FileName, true);
        }

        private void cmdClear_Click(object sender, EventArgs e)
        {
            Data.Clear();
        }

        private void cmdClearLog_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
        }
    }
}
