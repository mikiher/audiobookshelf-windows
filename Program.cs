using System;
using System.Windows.Forms;

namespace AudiobookshelfTray
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // SetProcessDPIAware() is required to prevent text controls from being blurry on high DPI screens.
            SetProcessDPIAware();
            Application.Run(new AppTray());
        }
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}