using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace MoneyReckoner
{
    public class Logger
    {
        public enum LoggerLevel
        {
            Info,
            Error
        }

        public class LoggerItem
        {
            private string _text;
            private LoggerLevel _level;

            public LoggerItem(string text, LoggerLevel level)
            {
                _text = text;
                _level = level;
            }

            public string Text { get => _text; }
            public LoggerLevel Level { get => _level; }

            public override string ToString()
            {
                return _text;
            }
        }

        private static ListBox _listBox = null;
        private static string _filename = null;
        public static LoggerLevel Level { get; set; }

        public static ListBox TheListBox
        {
            get => _listBox;
            set
            {
                _listBox = value;
                _listBox.DrawMode = DrawMode.OwnerDrawFixed;
                _listBox.DrawItem += listBox_DrawItem;
            }
        }

        private delegate void VoidStringType(string str, LoggerLevel level);

        public static void Info(string text)
        {
            if (Level > LoggerLevel.Info) return;

            if (TheListBox.InvokeRequired)
            {
                VoidStringType action = new VoidStringType(Trace);
                _listBox.Invoke(action, new object[] { text, LoggerLevel.Info });
            }
            else
            {
                Trace(text, LoggerLevel.Info);
            }
        }
        public static void Error(string text)
        {
            if (TheListBox.InvokeRequired)
            {
                VoidStringType action = new VoidStringType(Trace);
                _listBox.Invoke(action, new object[] { text, LoggerLevel.Error });
            }
            else
            {
                Trace("Error: " + text, LoggerLevel.Error);
            }
        }

        public static void WriteToFile(string filename = null)
        {
            if (filename == null)
            {
                // no filename provided, so prompt user for one
                SaveFileDialog dlg = new SaveFileDialog();
                if (_filename != null) dlg.FileName = _filename;
                dlg.OverwritePrompt = true;
                dlg.Filter = "Text file|*.txt";
                dlg.Title = "Money reckoner, save log to file";
                dlg.ShowDialog();

                if (dlg.FileName.Length == 0) return;
                filename = dlg.FileName;
            }

            _filename = filename;

            // write to log file
            StreamWriter sw = new StreamWriter(filename);
            for (int i = 0; i < _listBox.Items.Count; ++i)
            {
                sw.WriteLine(_listBox.Items[i]);
            }
            sw.Close();
        }

        public static void Clear()
        {
            _listBox.Items.Clear();
        }

        public static void Assert(bool test, string description)
        {
            if (!test) Error(description);
        }

        public static void Assert(string s1, string s2, string description)
        {
            if (s1.CompareTo(s2) != 0) Error(description);
        }

        private static DrawItemEventHandler dev = null;

        private static void Trace(string text, LoggerLevel level)
        {
            if (_listBox.Items.Count > 1000)
            {
                for (int i = 0; i < 100; ++i) _listBox.Items.RemoveAt(0);
            }
            _listBox.Items.Add(new LoggerItem(text, level));
            MakeLastLineVisible();
        }

        private static void MakeLastLineVisible()
        {
            int topIndex = _listBox.Items.Count -
                (_listBox.Height / _listBox.GetItemHeight(0));
            if (topIndex > 0) _listBox.TopIndex = topIndex;
        }

        private static void listBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            Graphics g = e.Graphics;
            //g.FillRectangle(new SolidBrush(Color.White), e.Bounds);
            ListBox lb = (ListBox)sender;
            LoggerItem li = (LoggerItem)lb.Items[e.Index];
            Color c = Color.Black;
            if (li.Level == LoggerLevel.Error) c = Color.Red;
            g.DrawString(lb.Items[e.Index].ToString(), e.Font, new SolidBrush(c), new PointF(e.Bounds.X, e.Bounds.Y));
            e.DrawFocusRectangle();
        }
    }
}
