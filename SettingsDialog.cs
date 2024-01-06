using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace AudiobookshelfTray
{
    public partial class SettingsDialog : Form
    {
        private readonly AppTray _app;
        private readonly string _initialPort;
        private readonly string _initialDataFolder;

        public SettingsDialog(AppTray app)
        {
            InitializeComponent();

            _app = app;
            _initialPort = _app.GetServerPort();
            _initialDataFolder = _app.GetServerDataDir();

            textBoxPort.Text = _initialPort;
            textBoxDataFolder.Text = _initialDataFolder;            
        }

        private void CancelClicked(object sender, EventArgs e)
        {

        }

        private void BrowseClicked(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new()
            {
                Description = "Select the folder where your server configuration and metadata should be stored.",
                SelectedPath = textBoxDataFolder.Text
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBoxDataFolder.Text = dialog.SelectedPath;
            }
        }

        private void SaveClicked(object sender, EventArgs e)
        {
            if (ValidatePort() && ValidateDataFolder())
            {
                if (textBoxDataFolder.Text != _initialDataFolder || textBoxPort.Text != _initialPort)
                {
                    _app.SaveServerPort(textBoxPort.Text);
                    _app.SaveServerDataDir(textBoxDataFolder.Text);
                    DialogResult result = MessageBox.Show("Changing server settings requires a server restart. Do you want to restart it now?",
                        "Audiobookshelf", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.No)
                        return;
                    _app.StopServerClicked(sender, e);
                    _app.StartServerClicked(sender, e);
                }
                this.Close();
            }
            else
            {
                MessageBox.Show("One of the server settings is invalid. Please correct it before saving.", "Audiobookshelf", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PortValidating(object sender, CancelEventArgs e)
        {
            if (!ValidatePort())
                e.Cancel = true;
        }

        private bool ValidatePort()
        {
            int portNumber;

            try
            {
                portNumber = Int32.Parse(textBoxPort.Text);

                // Ensure port number is within valid range
                if (portNumber < 1 || portNumber > 65535)
                {
                    errorProviderPort.SetError(labelPort, "Invalid port number. Please enter a value between 1 and 65535.");
                    return false;
                }
            }
            catch (FormatException)
            {
                errorProviderPort.SetError(labelPort, "Please enter a valid integer for the port number.");
                return false;
            }
            errorProviderPort.SetError(labelPort, "");
            return true;
        }

        private void PortTextChanged(object sender, EventArgs e)
        {
            ValidatePort();
        }

        private void DataFolderValidating(object sender, CancelEventArgs e)
        {
            if (!ValidateDataFolder())
                e.Cancel = true;
        }

        private bool ValidateDataFolder()
        {
            string datafolder = textBoxDataFolder.Text;
            if (!System.IO.Directory.Exists(datafolder))
            {
                errorProviderDataFolder.SetError(labelDataFolder, "Please enter a valid folder path.");
                return false;
            }
            errorProviderDataFolder.SetError(labelDataFolder, "");
            return true;
        }

        private void DataFolderTextChanged(object sender, EventArgs e)
        {
            ValidateDataFolder();
        }
    }
}
