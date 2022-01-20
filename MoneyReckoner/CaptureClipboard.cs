using System;
using System.Windows.Forms;

namespace MoneyReckoner
{ 
    class CaptureClipboard
    {
        private string _ctext;
        private Main _main;
        private Timer _timer;

        public void Initialise(Main main)
        {
            _main = main;
            _ctext = "";
            Clipboard.Clear();

            _timer = new Timer();
            _timer.Interval = 250;
            _timer.Enabled = true;
            _timer.Tick += new System.EventHandler(this._timer_Tick);
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                string ctext = Clipboard.GetText();

                if (_ctext.Length == 0 || ctext != _ctext)
                    Data.StatementCapture(ctext);

                _ctext = ctext;
            }
        }
    }
}
