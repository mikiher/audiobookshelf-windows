using AudiobookshelfTray.Properties;
using Microsoft.Win32;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AudiobookshelfTray
{
    public class AppTray : ApplicationContext
    {
        private readonly string _appName = "Audiobookshelf";
        private readonly string _serverFilename = "audiobookshelf.exe";
        private readonly string _trayAppName = "AudiobookshelfTray";
        private readonly string _repoOwner = "mikiher";
        private readonly string _repoName = "audiobookshelf-windows";

        private Process _serverProcess = null;
        private ServerLogs _serverLogsForm = null;
        private bool _shouldExit = false;
        private bool _runInstall = false;
        private string _installerPath;
        private readonly NotifyIcon _trayIcon;
        private readonly ToolStripMenuItem _stopServerMenuItem;
        private readonly ToolStripMenuItem _startServerMenuItem;
        private readonly ToolStripMenuItem _openServerMenuItem;
        private readonly ToolStripMenuItem _serverLogsMenuItem;
        private readonly ToolStripMenuItem _aboutMenuItem;
        private readonly ToolStripMenuItem _startAtLoginCheckboxMenuItem;
        private readonly ToolStripMenuItem _settingsMenuItem;
        private readonly ToolStripMenuItem _checkForUpdatesMenuItem;

        private readonly List<string> _serverLogsList = [];

        public AppTray()
        {
            _stopServerMenuItem = new ToolStripMenuItem("Stop Server", null, StopServerClicked) { Enabled = false };
            _startServerMenuItem = new ToolStripMenuItem("Start Server", null, StartServerClicked) { Enabled = false };
            _serverLogsMenuItem = new ToolStripMenuItem("Server Logs", null, ShowServerLogsClicked) { Enabled = false };
            _openServerMenuItem = new ToolStripMenuItem("Open Audiobookshelf...", null, OpenClicked) { Enabled = false };
            _openServerMenuItem.Font = new Font(_openServerMenuItem.Font.Name, _openServerMenuItem.Font.Size, FontStyle.Bold);
            _aboutMenuItem = new ToolStripMenuItem("About Audiobookshelf Server", null, AboutClicked);
            _startAtLoginCheckboxMenuItem = new ToolStripMenuItem("Start Audiobookshelf at Login") { CheckOnClick = true };
            _startAtLoginCheckboxMenuItem.CheckedChanged += StartAtLoginCheckedChanged;
            _startAtLoginCheckboxMenuItem.Checked = Settings.Default.StartAtLogin;
            _settingsMenuItem = new ToolStripMenuItem("Settings", null, SettingsClicked);
            _checkForUpdatesMenuItem = new ToolStripMenuItem("Check for Updates", null, CheckForUpdates);

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
                        _checkForUpdatesMenuItem,
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

            //_serverBinDir = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Audiobookshelf", "InstallDir",
            //    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", _appName)) as string;

            // Create a hidden window to handle WM_CLOSE messages
            MainForm = new Form
            {
                Text = _trayAppName,
                ShowInTaskbar = false,
                WindowState = FormWindowState.Minimized,
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                Opacity = 0,
            };
            MainForm.Load += (sender, e) => { if (_shouldExit) ExitClicked(sender, e); };

            Init();
        }

        public string GetServerDataDir()
        {
            string serverDataDir = Settings.Default.DataDir;
            string registryServerDataDir = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Audiobookshelf", "DataDir", null) as string;
            if (string.IsNullOrEmpty(serverDataDir))
            {
                string defaultDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _appName);
                serverDataDir = registryServerDataDir ?? defaultDataDir;
            }
            if (!Directory.Exists(serverDataDir))
            {
                try
                {
                    Directory.CreateDirectory(serverDataDir);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    MessageBox.Show("Failed to create server data directory at " + serverDataDir, "Audiobookshelf", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
            if (serverDataDir != registryServerDataDir || serverDataDir != Settings.Default.DataDir)
                SaveServerDataDir(serverDataDir);
            return serverDataDir;
        }

        public void SaveServerDataDir(string serverDataDir)
        {
            Settings.Default.DataDir = serverDataDir;
            Settings.Default.Save();
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Audiobookshelf", "DataDir", serverDataDir);
        }
        public string GetServerPort()
        {
            return Settings.Default.ServerPort;
        }

        public void SaveServerPort(string serverPort)
        {
            Settings.Default.ServerPort = serverPort;
            Settings.Default.Save();
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
            Debug.WriteLine("About to exit...");
            StopServer();
            if (_runInstall)
            {
                Process installerProcess = new()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        Arguments = "/SILENT",
                        FileName = _installerPath,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    },
                    EnableRaisingEvents = true
                };
                installerProcess.Start();
            }   
        }

        public void ExitClicked(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;
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
                Debug.WriteLine("Stopping server...");
                ProcessUtils.StopProcess(_serverProcess);
                _serverProcess = null;
                Debug.WriteLine("Server stopped");
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

            string serverDataDir = GetServerDataDir();
            if (serverDataDir == null)
                return false;

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

            Process.Start("http://localhost:" + Settings.Default.ServerPort);
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

        private async void CheckForUpdates(object sender, EventArgs e)
        {
            // Find latest release on GitHub
            GitHubClient client = new(new ProductHeaderValue(_trayAppName));
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll(_repoOwner, _repoName);
            Release latestRelease = releases[0];
            Debug.WriteLine("Latest release: " + latestRelease.TagName);
            Debug.WriteLine("Current release: " + Settings.Default.ServerVersion);

            if (latestRelease.TagName != Settings.Default.ServerVersion)
            {
                // Find installer asset
                ReleaseAsset exeAsset = latestRelease.Assets.First(asset => asset.Name.EndsWith(".exe"));
                if (exeAsset == null)
                {
                    Debug.WriteLine("No exe asset found");
                    MessageBox.Show("Failed to find installer for the latest release.", "Audiobookshelf", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Ask user if they want to download and install the new version
                if (MessageBox.Show("New version " + latestRelease.TagName + " available.\nDownload and install it?", "Audiobookshelf", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    // Download the new installer to a temp directory and run it
                    string tempDir = Path.Combine(Path.GetTempPath(), "Audiobookshelf");
                    if (!Directory.Exists(tempDir))
                    {
                        Directory.CreateDirectory(tempDir);
                    }
                    _installerPath = Path.Combine(tempDir, exeAsset.Name);
                    if (await DownloadInstaller(exeAsset.BrowserDownloadUrl))
                    {
                        MessageBox.Show("About to install new version. Audiobookshelf will exit now.", "Audiobookshelf", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _runInstall = true;
                        ExitClicked(sender, e);
                    }
                    else
                    {
                        Debug.WriteLine("Failed to download installer");
                        MessageBox.Show("Failed to download installer", "Audiobookshelf", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }                        
            }
            else
            {
                MessageBox.Show("No updates available", "Audiobookshelf", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async Task<bool> DownloadInstaller(string downloadUrl)
        {
            Debug.WriteLine("Downloading installer to " + _installerPath);
            System.Net.Http.HttpClient httpClient = new();
            System.Net.Http.HttpResponseMessage response = await httpClient.GetAsync(downloadUrl);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    using Stream stream = await response.Content.ReadAsStreamAsync();
                    using FileStream fileStream = new(_installerPath, System.IO.FileMode.Create, FileAccess.Write);
                    await stream.CopyToAsync(fileStream);
                    Debug.WriteLine("Downloaded installer to " + _installerPath);
                    fileStream.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to download installer: " + ex.ToString());
                    return false;
                }
            }
            else
            {
                Debug.WriteLine("Failed to download installer: " + response.StatusCode);
                return false;
            }
        }
    }
}
