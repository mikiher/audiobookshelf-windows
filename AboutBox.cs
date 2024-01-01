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
            Text = String.Format("About {0} Server", AssemblyTitle);
            labelProductName.Text = AssemblyProduct;
            labelVersion.Text = String.Format("Version {0}", AssemblyVersion);
            labelVersion.Click += (s, e) => System.Diagnostics.Process.Start("https://github.com/advplyr/audiobookshelf/releases/tag/" + AssemblyVersion);
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

        public string AssemblyVersion
        {
            get
            {
                return Settings.Default.ServerVersion;
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
