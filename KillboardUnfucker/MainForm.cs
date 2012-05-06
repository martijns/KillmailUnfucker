using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Media;

namespace KillmailUnfucker
{
    public partial class MainForm : Form
    {
        private int _killmailsUnfucked = 0;

        public MainForm()
        {
            InitializeComponent();
            lblMailsUnfucked.Text = _killmailsUnfucked + "";

            clipboardTimer.Tick += HandleTimerTick;
            clipboardTimer.Interval = 1000;
            clipboardTimer.Enabled = true;
        }

        void HandleTimerTick(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                string txt = Clipboard.GetText();
                if (IsAndFixedKillmail(ref txt))
                {
                    Clipboard.SetText(txt);

                    // Update UI
                    _killmailsUnfucked++;
                    lblMailsUnfucked.Text = _killmailsUnfucked + "";

                    // Beep optionally
                    if (chkBeep.Checked)
                        SystemSounds.Beep.Play();
                }
            }
        }

        private static string[] killmailCharacteristics = new string[] {
            "Victim:",
            "Corp:",
            "Alliance:",
            "Faction:",
            "Destroyed:",
            "System:",
            "Security:",
            "Damage Taken:"
        };

        protected bool IsAndFixedKillmail(ref string txt)
        {
            // High speec check for killmail characteristics
            foreach (string characteristic in killmailCharacteristics)
            {
                if (!txt.Contains(characteristic))
                    return false;
            }

            // Go through each line
            bool hasFixed = false;
            string[] lines = txt.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                // Find the Security: lines
                if (lines[i].Contains("Security:"))
                {
                    // This is already unfucked if it doesnt have a comma
                    if (!lines[i].Contains(","))
                        continue;

                    // Fix this line
                    lines[i] = lines[i].Replace(',', '.');
                    hasFixed = true;
                }
            }
            if (!hasFixed)
                return false;

            // Reassemble as one line
            txt = string.Join("\n", lines);
            return true;
        }

        private void HandleFormClosed(object sender, FormClosedEventArgs e)
        {
            clipboardTimer.Enabled = false;
            notifyIcon.Visible = false;
        }

        private void HandleExitButtonClick(object sender, EventArgs e)
        {
            Close();
        }

        private void HandleMinimizeToTrayButtonClick(object sender, EventArgs e)
        {
            MinimizeToTray();
        }

        private void HandleResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                MinimizeToTray();
            }
        }

        private void MinimizeToTray()
        {
            notifyIcon.Visible = true;
            Hide();
        }

        private void RestoreFromTray()
        {
            notifyIcon.Visible = false;
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void HandleTrayClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                RestoreFromTray();
            }
        }

        private void HandleTrayMenuShowClicked(object sender, EventArgs e)
        {
            RestoreFromTray();
        }

        private void HandleTrayMenuExitClicked(object sender, EventArgs e)
        {
            Close();
        }

        private void HandleAboutClick(object sender, EventArgs e)
        {
            MessageBox.Show(this,
                "Since CCP added localization to generated killmails in the Eve Online client, some " +
                "killboards will complain about malformed killmails. This utility monitores the clipboard and will " +
                "fix the killmail if any localized features are found.\r\n" +
                "\r\n" +
                "Currently it only replaced the comma with a dot in lines that indicate security.\r\n" +
                "\r\n" +
                "~ Martijn Stolk ~", "Killmail Unfucker");
        }

    }
}
