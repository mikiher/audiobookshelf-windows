using System;
using System.Threading;
using System.Windows.Forms;

namespace AudiobookshelfTray
{
    internal static class Program
    {
        static readonly Mutex _mutex = new(true, "AudiobookshelfTray");

        [STAThread]
        static void Main()
        {
            // SetProcessDPIAware() is required to prevent text controls from being blurry on high DPI screens.
            SetProcessDPIAware();
            if (_mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new AppTray());
                _mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("AudiobookshelfTray is already running.\nCheck the system tray", "Audiobookshelf", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}