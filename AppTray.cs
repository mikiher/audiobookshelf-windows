using AudiobookshelfTray.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

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
        private readonly string _serverFilename = "audiobookshelf.exe";

        private Process _serverProcess = null;
        private ServerLogs _serverLogsForm = null;
        private bool _shouldExit = false;
        private readonly NotifyIcon _trayIcon;
        private readonly ToolStripMenuItem _stopServerMenuItem;
        private readonly ToolStripMenuItem _startServerMenuItem;
        private readonly ToolStripMenuItem _openServerMenuItem;
        private readonly ToolStripMenuItem _serverLogsMenuItem;
        private readonly ToolStripMenuItem _aboutMenuItem;
        private readonly ToolStripMenuItem _startAtLoginCheckboxMenuItem;
        private readonly ToolStripMenuItem _settingsMenuItem;

        private readonly List<string> _serverLogsList = [];

        public AppTray()
        {
            _stopServerMenuItem = new ToolStripMenuItem("Stop Server", null, StopServerClicked) { Enabled = false };
            _startServerMenuItem = new ToolStripMenuItem("Start Server", null, StartServerClicked) { Enabled = false };
            _serverLogsMenuItem = new ToolStripMenuItem("Server Logs", null, ShowServerLogsClicked) { Enabled = false };
            _openServerMenuItem = new ToolStripMenuItem("Open Audiobookshelf...", null, OpenClicked) { Enabled = false };
            _openServerMenuItem.Font = new Font(_openServerMenuItem.Font.Name, _openServerMenuItem.Font.Size, System.Drawing.FontStyle.Bold);
            _aboutMenuItem = new ToolStripMenuItem("About Audiobookshelf Server", null, AboutClicked);
            _startAtLoginCheckboxMenuItem = new ToolStripMenuItem("Start Audiobookshelf at Login") { CheckOnClick = true };
            _startAtLoginCheckboxMenuItem.CheckedChanged += StartAtLoginCheckedChanged;
            _startAtLoginCheckboxMenuItem.Checked = Properties.Settings.Default.StartAtLogin;
            _settingsMenuItem = new ToolStripMenuItem("Settings", null, SettingsClicked);

            _trayIcon = new NotifyIcon()
            {
                Icon = Resources.AppIcon,
                ContextMenuStrip = new ContextMenuStrip()
                {
                    Items = {
                        _openServerMenuItem,
                        _startAtLoginCheckboxMenuItem,
                        _settingsMenuItem,
                        new ToolStripSeparator(),
                        _stopServerMenuItem,
                        _startServerMenuItem,
                        _serverLogsMenuItem,
                        new ToolStripSeparator(),
                        _aboutMenuItem,
                        new ToolStripSeparator(),
                        new ToolStripMenuItem("Exit", null, ExitClicked)
                    },
                    //Font = new Font("Segoe UI", 8.25f, System.Drawing.FontStyle.Regular)
                },
                Visible = true,
                Text = _appName
            };

            // Check if we need to upgrade settings from previous version
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            string serverDataDir = string.IsNullOrEmpty(Settings.Default.DataDir) ?
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Audiobookshelf", "DataDir",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _appName)) as string :
                Settings.Default.DataDir;
            if (serverDataDir != Settings.Default.DataDir)
            {
                Settings.Default.DataDir = serverDataDir;
                Settings.Default.Save();
            }

            //_serverBinDir = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Audiobookshelf", "InstallDir",
            //    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", _appName)) as string;

            // Create a hidden window to handle WM_CLOSE messages
            MainForm = new Form
            {
                Text = "AudiobookshelfTray",
                ShowInTaskbar = false,
                WindowState = FormWindowState.Minimized,
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                Opacity = 0,
            };
            MainForm.Load += (sender, e) => { if (_shouldExit) System.Windows.Forms.Application.Exit(); };

            Init();
        }

        private void SettingsClicked(object sender, EventArgs e)
        {
            SettingsDialog settingsDialog = new(this);
            settingsDialog.ShowDialog();
        }

        private void Init()
        {
            // Start server
            if (!StartServer())
                _shouldExit = true;

            _openServerMenuItem.Enabled = true;
            _serverLogsMenuItem.Enabled = true;
            _trayIcon.DoubleClick += OpenClicked;
            _trayIcon.BalloonTipClicked += BalloonTipClicked;

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
                Properties.Settings.Default.StartAtLogin = true;
            }
            else
            {
                Debug.WriteLine("Removing from startup");
                rk.DeleteValue("Audiobookshelf", false);
                Properties.Settings.Default.StartAtLogin = false;
            }
            Properties.Settings.Default.Save();
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

        private bool StartServer()
        {
            if (_serverProcess != null)
            {
                Debug.WriteLine("Server already started");
                return false;
            }

            string serverBinDir = System.Windows.Forms.Application.StartupPath;
            Debug.WriteLine("Server binary dir: " + serverBinDir);
            string serverBinPath = Path.Combine(serverBinDir, _serverFilename);
            Debug.WriteLine("Server binary path: " + serverBinPath);

            // Check if server binary exists
            if (!File.Exists(serverBinPath))
            {
                Debug.WriteLine("Server binary not found at " + serverBinPath);
                MessageBox.Show("Server binary not found at " + serverBinPath, "Audiobookshelf", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            string serverDataDir = Settings.Default.DataDir;
            Debug.WriteLine("Server data dir: " + serverBinDir);
            // Create data path directory if does not exist
            if (!Directory.Exists(serverDataDir))
            {
                Debug.WriteLine("Creating data path at " + serverDataDir);
                try
                {
                    Directory.CreateDirectory(serverDataDir);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    MessageBox.Show("Failed to create data path at " + serverDataDir, "Audiobookshelf", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            else
            {
                Debug.WriteLine("Abs data path already exists");
            }

            string configPath = Path.Combine(serverDataDir, "config");
            string metadataPath = Path.Combine(serverDataDir, "metadata");

            string serverPort = Settings.Default.ServerPort;

            Debug.WriteLine("Starting service");

            _serverProcess = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    Arguments = " -p " + serverPort + " --config " + configPath + " --metadata " + metadataPath + " --source windows",
                    FileName = serverBinPath,
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

            return true;
        }

        private void ServerExited(object sender, EventArgs e)
        {
            // check if this is happening in the UI thread
            if (_trayIcon.ContextMenuStrip.InvokeRequired)
            {
                _trayIcon.ContextMenuStrip.Invoke(new MethodInvoker(delegate
                {
                    ServerExited(sender, e);
                }));
                return;
            }

            Process process = sender as Process;

            Debug.WriteLine("sender exit code: " + process.ExitCode);

            // check if server exited with error
            if (process.ExitCode != 0)
            {
                Debug.WriteLine("Server exited with error code " + process.ExitCode);
                _trayIcon.ShowBalloonTip(500, "Audiobookshelf", "Server exited with error code " + process.ExitCode, ToolTipIcon.Error);
            }
            else
            {
                Debug.WriteLine("Server exited");
                _trayIcon.ShowBalloonTip(500, "Audiobookshelf", "Server exited", ToolTipIcon.Info);
            }
            // Fix up the context menu stuff
            _startServerMenuItem.Enabled = true;
            _stopServerMenuItem.Enabled = false;
        }

        private void OpenBrowser()
        {
            if (_serverProcess == null) return;

            System.Diagnostics.Process.Start("http://localhost:" + Settings.Default.ServerPort);
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
                    Properties.Settings.Default.ServerVersion = match.Value;
                    Properties.Settings.Default.Save();
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
