using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

public class DismissableMessageBox
{
    private const int IDNO = 7; // The ID for the 'No' button
    private string _caption;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool EndDialog(IntPtr hDlg, IntPtr nResult);

    public DismissableMessageBox(string caption)
    {
        _caption = caption;
    }

    public DialogResult Show(string text, MessageBoxButtons buttons, MessageBoxIcon icon = MessageBoxIcon.None)
    {
        return MessageBox.Show(text, _caption, buttons, icon);
    }

    public void Dismiss(int buttonId = IDNO)
    {
        IntPtr hWnd = FindWindow(null, _caption); // Find the MessageBox by its title
        if (hWnd != IntPtr.Zero)
        {
            EndDialog(hWnd, (IntPtr)buttonId); // Close the MessageBox with the specified result
        }
    }
}
