using System.Net.Sockets;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
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
                if (portNumber < 2 || portNumber > 65535 ) //consider changing this to the conventional 1024-49152. Also ports 0 and 1 are reserved.
                {
                    errorProviderPort.SetError(labelPort, "Invalid port number. Please enter a value between 1 and 65535.");
                    return false;
                }
                string processName = GetProcessUsingPort(portNumber);
                if ((IsPortFree(portNumber) == false | processName != "audiobookshelf") != true)
                {
                    // Port is in use, show error message
                    errorProviderPort.SetError(labelPort, $"Port {portNumber} is already in use by {processName}.");
                    return false;
                }
                else if (IsPortFree(portNumber) == false && processName == string.Empty) //edge case
                {
                    errorProviderPort.SetError(labelPort, $"Port {portNumber} is already in use by {processName}.");
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

       private string GetProcessUsingPort(int port)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c for /f \"skip=1 tokens=5\" %a in ('netstat -ano ^| findstr :\"{port}[^0-9]\"') do @for /f \"tokens=1\" %b in ('tasklist /FI \"PID eq %a\" ^| more ^| findstr /V \"Image\" ^| findstr /V \"=\"') do @echo %b",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            return output;
        }

        bool IsPortFree(int port)
        {
            try
            {
                // Attempt to bind to the port on localhost (127.0.0.1)
                var tcpListener = new TcpListener(System.Net.IPAddress.Loopback, port);
                try
                {
                    tcpListener.Start();
                    return true; // Port is free
                }
                finally
                {
                    tcpListener.Stop();
                }
            }
            catch (SocketException)
            {
                return false; // Port is in use
            }
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
