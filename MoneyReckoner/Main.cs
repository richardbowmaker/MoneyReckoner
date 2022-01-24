using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;


// improve displayed output

namespace MoneyReckoner
{
    public partial class Main : Form
    {
        public static Main _this;
        private CaptureClipboard _captureClipboard;
        private string _workingFolder;
        const string _loadFile = "statement.txt";

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            _this = this;
            Logger.TheListBox = lstLogger;
            
            _captureClipboard = new CaptureClipboard();
            _captureClipboard.Initialise(this);

            chkCashPlus.Enabled = false;
            chkCashPlus.Checked = false;
            chkSantanderCurrent.Enabled = false;
            chkSantanderCurrent.Checked = false;
            chkSantanderCredit.Enabled = false;
            chkSantanderCredit.Checked = false;

            // determine the working directory
            string cd = Directory.GetCurrentDirectory();
            if (Debugger.IsAttached)
            {
                DirectoryInfo di = Directory.GetParent(cd);
                di = Directory.GetParent(di.FullName);
                di = Directory.GetParent(di.FullName);
                _workingFolder = di.FullName;
            }
            else
            {
                DirectoryInfo di = Directory.GetParent(cd);
                _workingFolder = di.FullName;
            }
            Logger.Info("Working folder = " + _workingFolder);

            // load curernt statement
            Data.Serialise(_workingFolder + "\\" + _loadFile, true);
            Data.StatementSummaryToLog();
        }

        public void SetCashplus()
        {
            chkCashPlus.Checked = true;
        }

        public void SetSantanderCurrent()
        {
            chkSantanderCurrent.Checked = true;
        }
        public void SetSantanderCredit()
        {
            chkSantanderCredit.Checked = true;
        }

        private void cmdSummaries_Click(object sender, EventArgs e)
        {
            Data.StatementSummaryToLog();
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
            ClearLog();
        }

        public void ClearLog()
        {
            Logger.Clear();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!Data.IsDirty || Data.Count == 0) return;

            DialogResult dr = MessageBox.Show("Do you want to save the current statement", "Money Reckoner", MessageBoxButtons.YesNoCancel);

            switch (dr)
            {
                case DialogResult.Yes:
                    Data.Serialise(_workingFolder + "\\" + _loadFile, false);
                    break;
                case DialogResult.Cancel:
                    e.Cancel = true;
                    break;
                default:
                    break;
            }
        }
    }
}
