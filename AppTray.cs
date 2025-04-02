using AudiobookshelfTray.Properties;
using Microsoft.Win32;
using NLog;
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
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly Logger _serverLogger = LogManager.GetLogger("Server");
        private readonly string _appName = "Audiobookshelf";
        private readonly string _serverFilename = "audiobookshelf.exe";
        private readonly string _trayAppName = "AudiobookshelfTray";
        private readonly string _repoOwner = "mikiher";
        private readonly string _repoName = "audiobookshelf-windows";
        private readonly string _appVersion;
        private readonly System.Timers.Timer _dailyTimer = new();

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
        private readonly ToolStripMenuItem _autoCheckForUpdatesCheckboxMenuItem;
        private readonly ToolStripMenuItem _settingsMenuItem;
        private readonly ToolStripMenuItem _checkForUpdatesMenuItem;

        private readonly List<string> _serverLogsList = [];

        private DismissableMessageBox _newVersionAvailableDialog = null;

        private const string REGISTRY_KEY = @"HKEY_CURRENT_USER\Software\Audiobookshelf";

        public AppTray()
        {            // First check if we need to upgrade settings from previous versio// Then migrate settings to registry
            MigrateSettingsToRegistry();

            _appVersion = GetRegistryValue("AppVersion", "");

            _stopServerMenuItem = new ToolStripMenuItem("Stop Server", null, StopServerClicked) { Enabled = false };
            _startServerMenuItem = new ToolStripMenuItem("Start Server", null, StartServerClicked) { Enabled = false };
            _serverLogsMenuItem = new ToolStripMenuItem("Server Logs", null, ShowServerLogsClicked) { Enabled = false };
            _openServerMenuItem = new ToolStripMenuItem("Open Audiobookshelf...", null, OpenClicked) { Enabled = false };
            _openServerMenuItem.Font = new Font(_openServerMenuItem.Font.Name, _openServerMenuItem.Font.Size, FontStyle.Bold);
            _aboutMenuItem = new ToolStripMenuItem("About Audiobookshelf Server", null, AboutClicked);
            _startAtLoginCheckboxMenuItem = new ToolStripMenuItem("Start Audiobookshelf at Login") { CheckOnClick = true };
            _startAtLoginCheckboxMenuItem.CheckedChanged += StartAtLoginCheckedChanged;
            _startAtLoginCheckboxMenuItem.Checked = GetRegistryValue("StartAtLogin", true);
            _autoCheckForUpdatesCheckboxMenuItem = new ToolStripMenuItem("Automatically Check for Updates") { CheckOnClick = true };
            _autoCheckForUpdatesCheckboxMenuItem.CheckedChanged += AutoCheckForUpdatesChanged;
            _autoCheckForUpdatesCheckboxMenuItem.Checked = GetRegistryValue("AutoCheckForUpdates", true);
            _settingsMenuItem = new ToolStripMenuItem("Settings", null, SettingsClicked);
            _checkForUpdatesMenuItem = new ToolStripMenuItem("Check for Updates", null, CheckForUpdates);
            _dailyTimer.Interval = 24 * 60 * 60 * 1000; // 24 hours
            _dailyTimer.Elapsed += CheckForUpdates;

            _trayIcon = new NotifyIcon()
            {
                Icon = Resources.AppIcon,
                ContextMenuStrip = new ContextMenuStrip()
                {
                    Items = {
                        _openServerMenuItem,
                        _startAtLoginCheckboxMenuItem,
                        _autoCheckForUpdatesCheckboxMenuItem,
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

        private void AutoCheckForUpdatesChanged(object sender, EventArgs e)
        {
            _logger.Debug("AutoCheckForUpdatesChanged");
            SetRegistryValue("AutoCheckForUpdates", _autoCheckForUpdatesCheckboxMenuItem.Checked);

            if (_autoCheckForUpdatesCheckboxMenuItem.Checked)
            {
                CheckForUpdates(sender, e);
                _dailyTimer.Start();
            }
            else
            {
                _dailyTimer.Stop();
            }
        }
        public string GetServerFileName()
        {
            return _serverFilename;
        }
        public string GetServerDataDir()
        {
            string serverDataDir = GetRegistryValue("DataDir", "");
            bool updated = false;

            if (string.IsNullOrEmpty(serverDataDir))
            {
                string defaultDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _appName);
                serverDataDir = defaultDataDir;
                updated = true;
            }
            if (!Directory.Exists(serverDataDir))
            {
                try
                {
                    Directory.CreateDirectory(serverDataDir);
                }
                catch (Exception e)
                {
                    _logger.Error(e.ToString());
                    MessageBox.Show("Failed to create server data directory at " + serverDataDir, "Audiobookshelf", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
            if (updated && serverDataDir != null)
                SaveServerDataDir(serverDataDir);
            return serverDataDir;
        }

        public void SaveServerDataDir(string serverDataDir)
        {
            SetRegistryValue("DataDir", serverDataDir);
        }

        public string GetServerPort()
        {
            return GetRegistryValue("ServerPort", "13378");
        }

        public void SaveServerPort(string serverPort)
        {
            SetRegistryValue("ServerPort", serverPort);
        }

        private void SettingsClicked(object sender, EventArgs e)
        {
            SettingsDialog settingsDialog = new(this, _serverFilename);
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
                _logger.Debug("Adding to startup");
                rk.SetValue("Audiobookshelf", System.Windows.Forms.Application.ExecutablePath);
                SetRegistryValue("StartAtLogin", true);
            }
            else
            {
                _logger.Debug("Removing from startup");
                rk.DeleteValue("Audiobookshelf", false);
                SetRegistryValue("StartAtLogin", false);
            }
        }

        private void AboutClicked(object sender, EventArgs e)
        {
            AboutBox aboutBox = new(this);
            aboutBox.ShowDialog();
        }

        private void ApplicationExited(object sender, EventArgs e)
        {
            _logger.Debug("About to exit...");
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
                _logger.Debug("Stopping server...");
                ProcessUtils.StopProcess(_serverProcess);
                _serverProcess = null;
                _logger.Debug("Server stopped");
            }
        }

        private bool StartServer()
        {
            if (_serverProcess != null)
            {
                _logger.Debug("Server already started");
                return false;
            }

            string serverBinDir = System.Windows.Forms.Application.StartupPath;
            _logger.Debug("Server binary dir: " + serverBinDir);
            string serverBinPath = Path.Combine(serverBinDir, _serverFilename);
            _logger.Debug("Server binary path: " + serverBinPath);

            // Check if server binary exists
            if (!File.Exists(serverBinPath))
            {
                _logger.Error("Server binary not found at " + serverBinPath);
                MessageBox.Show("Server binary not found at " + serverBinPath, "Audiobookshelf", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            string serverDataDir = GetServerDataDir();
            if (serverDataDir == null)
                return false;

            string configPath = Path.Combine(serverDataDir, "config");
            string metadataPath = Path.Combine(serverDataDir, "metadata");

            string serverPort = GetServerPort();

            _logger.Debug("Starting service");

            _serverProcess = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    Arguments = " -p " + serverPort + " --config \"" + configPath + "\" --metadata \"" + metadataPath + "\" --source windows",
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

            _logger.Debug("sender exit code: " + process.ExitCode);

            // check if server exited with error
            if (process.ExitCode != 0)
            {
                _logger.Error("Server exited with error code " + process.ExitCode);
                _trayIcon.ShowBalloonTip(500, "Audiobookshelf", "Server exited with error code " + process.ExitCode, ToolTipIcon.Error);
            }
            else
            {
                _logger.Debug("Server exited");
                _trayIcon.ShowBalloonTip(500, "Audiobookshelf", "Server exited", ToolTipIcon.Info);
            }
            // Fix up the context menu stuff
            _startServerMenuItem.Enabled = true;
            _stopServerMenuItem.Enabled = false;
        }

        private void OpenBrowser()
        {
            if (_serverProcess == null) return;

            Process.Start("http://localhost:" + GetServerPort());
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
                    SetRegistryValue("ServerVersion", match.Value);
                }
                else
                {
                    _logger.Error("Failed to parse server version from init log line " + outLine.Data);
                }
            }

            _serverLogger.Debug(outLine.Data);
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
            _logger.Debug("Latest release: " + latestRelease.TagName);
            _logger.Debug("Current release: " + _appVersion);

            if(_newVersionAvailableDialog != null)
            {
                _newVersionAvailableDialog.Dismiss();
            }

            if (latestRelease.TagName != _appVersion)
            {
                // Find installer asset
                ReleaseAsset exeAsset = latestRelease.Assets.First(asset => asset.Name.EndsWith(".exe"));
                if (exeAsset == null)
                {
                    _logger.Error("No exe asset found");
                    MessageBox.Show("Failed to find installer for the latest release.", "Audiobookshelf", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Ask user if they want to download and install the new version
                _newVersionAvailableDialog = new DismissableMessageBox("Audiobookshelf Update");
                DialogResult result = _newVersionAvailableDialog.Show("A New version " + latestRelease.TagName + " is available.\nDownload and install it?", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                _newVersionAvailableDialog = null;
                if (result == DialogResult.Yes)
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
                        _logger.Error("Failed to download installer");
                        MessageBox.Show("Failed to download installer", "Audiobookshelf", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                // silent if called from timer or autoCheckForUpdates checkbox
                if (sender == _checkForUpdatesMenuItem)
                {
                    MessageBox.Show("No updates available", "Audiobookshelf", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }
        }

        private async Task<bool> DownloadInstaller(string downloadUrl)
        {
            _logger.Debug("Downloading installer to " + _installerPath);
            System.Net.Http.HttpClient httpClient = new();
            System.Net.Http.HttpResponseMessage response = await httpClient.GetAsync(downloadUrl);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    using Stream stream = await response.Content.ReadAsStreamAsync();
                    using FileStream fileStream = new(_installerPath, System.IO.FileMode.Create, FileAccess.Write);
                    await stream.CopyToAsync(fileStream);
                    _logger.Debug("Downloaded installer to " + _installerPath);
                    fileStream.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to download installer: " + ex.ToString());
                    return false;
                }
            }
            else
            {
                _logger.Error("Failed to download installer: " + response.StatusCode);
                return false;
            }
        }

        private void MigrateSettingsToRegistry()
        {            // Check if we've already migrated
            try
            {
                if (GetRegistryValue("SettingsMigrated", false))
                {
                    return;
                }

                _logger.Debug("Migrating settings to registry...");

                if (Settings.Default.UpgradeRequired)
                {
                    _logger.Debug("Upgrading settings");
                    Settings.Default.Upgrade();
                    Settings.Default.UpgradeRequired = false;
                    Settings.Default.Save();
                    _logger.Debug("Settings upgraded");
                }
                // Migrate all settings to registry
                _logger.Debug("Setting registry value StartAtLogin: " + Settings.Default.StartAtLogin);
                SetRegistryValue("StartAtLogin", Settings.Default.StartAtLogin);
                _logger.Debug("Setting registry value AutoCheckForUpdates: " + Settings.Default.AutoCheckForUpdates);
                SetRegistryValue("AutoCheckForUpdates", Settings.Default.AutoCheckForUpdates);
                _logger.Debug("Setting registry value ServerPort: " + Settings.Default.ServerPort);
                SetRegistryValue("ServerPort", Settings.Default.ServerPort);
                _logger.Debug("Setting registry value DataDir: " + Settings.Default.DataDir);
                SetRegistryValue("ServerVersion", Settings.Default.ServerVersion);

                // Mark migration as complete
                SetRegistryValue("SettingsMigrated", true);
                _logger.Debug("Settings migration completed successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to migrate settings to registry: {ex}");
                return;
            }
        }

        public T GetRegistryValue<T>(string name, T defaultValue)
        {
            try
            {
                var value = Registry.GetValue(REGISTRY_KEY, name, defaultValue);
                if (value == null)
                    return defaultValue;

                if (value is int intValue)
                {
                    if (typeof(T) == typeof(bool))
                    {
                        return (T)(object)(intValue != 0);
                    }
                }
                else if (value is string stringValue)
                {
                    if (typeof(T) == typeof(string))
                    {
                        return (T)(object)stringValue;
                    }
                }
                else if (value is bool boolValue)
                {
                    if (typeof(T) == typeof(bool))
                    {
                        return (T)(object)boolValue;
                    }
                }

                _logger.Debug("Registry " + name + " value is unknown type: " + value.GetType().Name);
                return defaultValue;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to read registry value {name}: {ex}. Returning default value '{defaultValue}'");
                return defaultValue;
            }
        }

        private void SetRegistryValue(string name, object value)
        {
            try
            {
                if (value is bool boolValue)
                {
                    Registry.SetValue(REGISTRY_KEY, name, boolValue ? 1 : 0, RegistryValueKind.DWord);
                }
                else
                {
                    Registry.SetValue(REGISTRY_KEY, name, value);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to set registry value {name} to {value}: {ex}");
            }
        }
    }
}
