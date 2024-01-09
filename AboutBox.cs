using AudiobookshelfTray.Properties;
using System;
using System.Windows.Forms;

namespace AudiobookshelfTray
{
    partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
            Text = String.Format("About {0}", AssemblyTitle);
            labelProductName.Text = AssemblyProduct;
            labelServerVersion.Text = String.Format("Server Version {0}", AssemblyServerVersion);
            labelServerVersion.Click += (s, e) => System.Diagnostics.Process.Start("https://github.com/advplyr/audiobookshelf/releases/tag/" + AssemblyServerVersion);
            labelAppVersion.Text = String.Format("App Version {0}", AssemblyAppVersion);
            labelAppVersion.Click += (s, e) => System.Diagnostics.Process.Start("https://github.com/mikiher/audiobookshelf-windows/releases/tag/" + AssemblyAppVersion);
            labelURL.Text = "audiobookshelf.org";
            labelURL.Click += (s, e) => System.Diagnostics.Process.Start("https://www.audiobookshelf.org/");
            StartPosition = FormStartPosition.CenterScreen;
        }

        #region Assembly Attribute Accessors

        public string AssemblyProduct
        {
            get
            {
                return string.Format("{0}\nSelf-hosted audiobook and podcast server", Settings.Default.ServerName);
            }
        }

        public string AssemblyServerVersion
        {
            get
            {
                return String.IsNullOrEmpty(Settings.Default.ServerVersion) ? "unknown" : Settings.Default.ServerVersion;
            }
        }

        public string AssemblyAppVersion
        {
            get
            {
                return String.IsNullOrEmpty(Settings.Default.AppVersion) ? "unknown" : Settings.Default.AppVersion;
            }
        }

        public string AssemblyTitle
        {
            get
            {
                return Settings.Default.ServerName;
            }
        }

        #endregion
    }
}
