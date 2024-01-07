using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NLog;

namespace AudiobookshelfTray
{
    internal class ProcessUtils
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GenerateConsoleCtrlEvent(CtrlTypes dwCtrlEvent, uint dwProcessGroupId);

        [DllImport("Kernel32", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool add);

        private enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        private delegate bool HandlerRoutine(CtrlTypes CtrlType);

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Tries to stop a process by sending a CTRL_C_EVENT to its console.
        /// This allows the process to perform cleanup operations before exiting, unlike Process.Kill().
        /// If the process does not exit within a reasonable amount of time, Process.Kill() is called.
        /// </summary>
        public static void StopProcess(Process process)
        {
            if (process == null || process.HasExited)
            {
                return;
            }

            int pid = process.Id;

            if (AttachConsole((uint)pid))
            {
                SetConsoleCtrlHandler(null, true);
                bool ctrlCSent = GenerateConsoleCtrlEvent(CtrlTypes.CTRL_C_EVENT, 0);
                if (ctrlCSent)
                {
                    _logger.Debug("Sent Ctrl+C to process. Waiting for it to exit");
                    try
                    {
                        if (!process.WaitForExit(8000))
                        {
                            _logger.Error("Process did not exit within 8 seconds");
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Exception thrown by Process.WaitForExit: {e}");
                    }
                }
                SetConsoleCtrlHandler(null, false);
                FreeConsole();
            }

            if (!process.HasExited)
            {
                _logger.Error("Failed to send Ctrl+C to process. Killing it instead");
                try
                {
                    process.Kill();
                    process.WaitForExit();
                }
                catch (Exception e)
                {
                    _logger.Error($"Exception thrown by Process.Kill: {e}");
                }
            }
        }
    }
}
