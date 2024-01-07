using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using NLog;

namespace AudiobookshelfTray
{
    public partial class ServerLogs : Form
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public ServerLogs()
        {
            InitializeComponent();

            // handle context menu item clicks
            selectAllToolStripMenuItem.Click += new EventHandler(SelectAllToolStripMenuItem_Click);
            copySelectedToolStripMenuItem.Click += new EventHandler(CopySelectedToolStripMenuItem_Click);
        }

        public void SetLogs(List<string> logLines)
        {
            _logger.Debug("Setting Logs " + logLines.Count);
            string[] linesArray = [.. logLines];

            if (linesArray != null && linesArray.Length > 0)
            {
                logsListBox.Invoke((MethodInvoker)delegate
                {
                    logsListBox.Items.AddRange(linesArray);
                    logsListBox.SelectedIndex = logsListBox.Items.Count - 1;
                });

            }
            else
            {
                _logger.Error("Error: Invalid logLines");
            }
        }

        public void AddLogLine(string line)
        {
            if (line == "" || line == null) return;

            logsListBox.Invoke((MethodInvoker)delegate
            {
                logsListBox.Items.Add(line);
                logsListBox.SelectedIndex = logsListBox.Items.Count - 1;
            });
        }

        private void CopySelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // copy selected items to clipboard
            StringBuilder sb = new();
            foreach (string s in logsListBox.SelectedItems)
            {
                sb.AppendLine(s);
            }
            Clipboard.SetText(sb.ToString());
        }

        private void SelectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // select all items in the listbox
            logsListBox.BeginUpdate();
            for (int i = 0; i < logsListBox.Items.Count; i++)
            {
                logsListBox.SetSelected(i, true);
            }
            logsListBox.EndUpdate();
        }
    }
}
