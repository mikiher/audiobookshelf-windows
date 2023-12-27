using System;
using System.Diagnostics;
using System.Windows.Forms;
using AudiobookshelfTray.Properties;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Octokit;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Drawing;

namespace AudiobookshelfTray
{
    class ReleaseCliAsset
    {
        public string Tag { get; set; }
        public string DownloadUrl { get; set; }
        public string Url { get; set; }
    }

    public class AppTray : ApplicationContext
    {
        private readonly string _appName = "Audiobookshelf";
        private readonly string _serverPort = "13378";
        private readonly string _serverFilename = "server.exe";
        private readonly string _serverDataDir;
        private readonly string _serverBinDir;
        private readonly string _serverBinPath;

        private Process _serverProcess = null;
        private ServerLogs _serverLogsForm = null;

        private readonly NotifyIcon _trayIcon;
        private readonly ToolStripMenuItem _stopServerMenuItem;
        private readonly ToolStripMenuItem _startServerMenuItem;
        private readonly ToolStripMenuItem _openServerMenuItem;
        private readonly ToolStripMenuItem _serverLogsMenuItem;
        private readonly ToolStripMenuItem _aboutMenuItem;
        private readonly ToolStripMenuItem _startAtLoginCheckboxMenuItem;

        private readonly List<string> _serverLogsList = [];

        public AppTray()
        {
            _stopServerMenuItem = new ToolStripMenuItem("Stop Server", null, StopServerClicked) { Enabled = false };
            _startServerMenuItem = new ToolStripMenuItem("Start Server", null, StartServerClicked) { Enabled = false };
            _serverLogsMenuItem = new ToolStripMenuItem("Server Logs", null, ShowServerLogsClicked) { Enabled = false};
            _openServerMenuItem = new ToolStripMenuItem("Open Audiobookshelf...", null, OpenClicked) { Enabled = false };
            _openServerMenuItem.Font = new Font(_openServerMenuItem.Font.Name, _openServerMenuItem.Font.Size, System.Drawing.FontStyle.Bold);
            _aboutMenuItem = new ToolStripMenuItem("About Audiobookshelf Server", null, AboutClicked);
            _startAtLoginCheckboxMenuItem = new ToolStripMenuItem("Start Audiobookshelf at Login") { CheckOnClick = true };
            _startAtLoginCheckboxMenuItem.CheckedChanged += StartAtLoginCheckedChanged;
            _startAtLoginCheckboxMenuItem.Checked = Settings.Default.StartAtLogin;

            _trayIcon = new NotifyIcon()
            {
                Icon = Resources.AppIcon,
                ContextMenuStrip = new ContextMenuStrip()
                {
                    Items = {
                        _openServerMenuItem,
                        _startAtLoginCheckboxMenuItem,
                        new ToolStripSeparator(),
                        _stopServerMenuItem,
                        _startServerMenuItem,
                        _serverLogsMenuItem,
                        new ToolStripSeparator(),
                        _aboutMenuItem,
                        new ToolStripSeparator(),
                        new ToolStripMenuItem("Exit", null, ExitClicked)
                    },
                    Font = new Font("Segoe UI", 8.25f, System.Drawing.FontStyle.Regular)
                },
                Visible = true,
                Text = _appName
            };

            _trayIcon.DoubleClick += OpenClicked;
            _trayIcon.BalloonTipClicked += BalloonTipClicked;

            _serverDataDir = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Audiobookshelf", "DataDir",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _appName)) as string;
            _serverBinDir = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Audiobookshelf", "InstallDir",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), _appName)) as string;


            // Create data path directory if does not exist
            if (!Directory.Exists(_serverDataDir))
            {
                Debug.WriteLine("Creating data path at " + _serverDataDir);
                try
                {
                    Directory.CreateDirectory(_serverDataDir);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    MessageBox.Show("Failed to create data path at " + _serverDataDir, "Audiobookshelf", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                Debug.WriteLine("Abs data path already exists");
            }

            _serverBinPath = Path.Combine(_serverBinDir, _serverFilename);

            Debug.WriteLine("Server data dir: " + _serverDataDir);
            Debug.WriteLine("Server binary dir: " + _serverBinDir);

            
            // Create a hidden window to handle WM_CLOSE messages
            MainForm = new Form
            {
                Text = "AudiobookshelfTray",
                ShowInTaskbar = false,
                WindowState = FormWindowState.Minimized,
                FormBorderStyle = FormBorderStyle.FixedToolWindow
            };
            

            Init();
        }

        private void Init()
        {          
            // Start server
            StartServer();

            _openServerMenuItem.Enabled = true;
            _serverLogsMenuItem.Enabled = true;

            // listen to wm_close
            System.Windows.Forms.Application.ApplicationExit += ApplicationExited;
        }


        private void StartAtLoginCheckedChanged(object sender, EventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (_startAtLoginCheckboxMenuItem.Checked)
            {
                Debug.WriteLine("Adding to startup");
                rk.SetValue("Audiobookshelf", System.Windows.Forms.Application.ExecutablePath);
                Settings.Default.StartAtLogin = true;
            }
            else
            {
                Debug.WriteLine("Removing from startup");
                rk.DeleteValue("Audiobookshelf", false);
                Settings.Default.StartAtLogin = false;
            }
            Settings.Default.Save();
        }

        private void AboutClicked(object sender, EventArgs e)
        {
            AboutBox aboutBox = new();
            aboutBox.ShowDialog();
        }

        private void ApplicationExited(object sender, EventArgs e)
        {
            Debug.WriteLine("About to exit. Stopping server");
            StopServer();
            _trayIcon.Visible = false;
        }

        public void ExitClicked(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        public void StopServerClicked(object sender, EventArgs e)
        {
            StopServer();
        }

        public void StartServerClicked(object sender, EventArgs e)
        {
            StartServer();
        }

        public void OpenClicked(object sender, EventArgs e)
        {
            // Server already started, 
            if (_serverProcess != null)
            {
                // just open the browser.
                OpenBrowser();
            }
            
            // Server not started, 
            else
            {
                // ask master if we should start it.
                if (MessageBox.Show("Server not started, start server?", "Audiobookshelf", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    // Lets do what our master told us to do.
                    StartServer();
                };
            }
        }

        public void ShowServerLogsClicked(object sender, EventArgs e)
        {
            _serverLogsForm = new ServerLogs();
            _serverLogsForm.Show();
            _serverLogsForm.SetLogs(_serverLogsList);
        }

        public void BalloonTipClicked(object sender, EventArgs e)
        {
            OpenBrowser();
        }

        private void StopServer()
        {
            if (_serverProcess != null)
            {
                ProcessUtils.StopProcess(_serverProcess);
                _serverProcess = null;
            }
        }

        private void StartServer()
        {
            if (_serverProcess != null)
            {
                Debug.WriteLine("Server already started");
                return;
            }
            Debug.WriteLine("Starting service");

            string configPath = Path.Combine(_serverDataDir, "config");
            string metadataPath = Path.Combine(_serverDataDir, "metadata");

            _serverProcess = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    Arguments = " -p " + _serverPort + " --config " + configPath + " --metadata " + metadataPath + " --source windows",
                    FileName = _serverBinPath,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };

            _serverProcess.OutputDataReceived += HandleServerOutput;
            _serverProcess.ErrorDataReceived += HandleServerOutput;
            _serverProcess.Exited += ServerExited;

            // Start the ABS Server process.
            _serverProcess.Start();

            _serverProcess.BeginOutputReadLine();
            _serverProcess.BeginErrorReadLine();
                

            // Show an alert that we started the server.
            _trayIcon.ShowBalloonTip(500, "Audiobookshelf", "Server started", ToolTipIcon.Info);

            // Fix up the context menu stuff
            _startServerMenuItem.Enabled = false;
            _stopServerMenuItem.Enabled = true;
        }

        private void ServerExited(object sender, EventArgs e)
        {
            // check if this is happening in the UI thread
            if(_trayIcon.ContextMenuStrip.InvokeRequired)
            {
                _trayIcon.ContextMenuStrip.Invoke(new MethodInvoker(delegate
                {
                    ServerExited(sender, e);
                }));
                return;
            }

            // check if server exited with error
            if (_serverProcess.ExitCode != 0)
            {
                Debug.WriteLine("Server exited with error code " + _serverProcess.ExitCode);
                _trayIcon.ShowBalloonTip(500, "Audiobookshelf", "Server exited with error code " + _serverProcess.ExitCode, ToolTipIcon.Error);
            }
            else
            {
                Debug.WriteLine("Server exited");
                _trayIcon.ShowBalloonTip(500, "Audiobookshelf", "Server exited", ToolTipIcon.Info);
            }
            // Fix up the context menu stuff
            _startServerMenuItem.Enabled = true;
            _stopServerMenuItem.Enabled = false;
            _serverProcess = null;
        }

        private void OpenBrowser()
        {
            if (_serverProcess == null) return;

            System.Diagnostics.Process.Start("http://localhost:" + _serverPort);
        }

        // Why??
        private void HandleServerOutput(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (outLine.Data == null || outLine.Data == "")
            {
                return;
            }
            
            if (outLine.Data.Contains("[Server] Init"))
            {
                // Extract server version from init log line
                System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(outLine.Data, @"v\d+\.\d+\.\d+$");
                if (match.Success)
                {
                    Settings.Default.ServerVersion = match.Value;
                    Settings.Default.Save();
                }
                else
                {
                    Debug.WriteLine("Failed to parse server version from init log line " + outLine.Data);
                }
            }

            Debug.WriteLine(outLine.Data);
            _serverLogsList.Add(outLine.Data);

            if (_serverLogsForm != null && !_serverLogsForm.IsDisposed)
            {
                // Server logs form is open add line
                _serverLogsForm.AddLogLine(outLine.Data);
            }
        }
    }
}
