using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
namespace AudiobookshelfTray
{
    internal class NetworkUtils
    {
       public static string GetProcessNameUsingPortId(int port)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c for /f \"skip=1 tokens=5\" %a in ('netstat -ano ^| findstr :\"{port}[^0-9]\"') do @for /f \"tokens=1\" %b in ('tasklist /FI \"PID eq %a\" ^| more ^| findstr /V \"Image\" ^| findstr /V \"=\"') do @echo %b",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            return output;
        }

        public static bool IsPortFree(int port)
        {
            try
            {
                // Attempt to bind to the port on localhost (127.0.0.1)
                var tcpListener = new TcpListener(System.Net.IPAddress.Loopback, port);
                try
                {
                    tcpListener.Start();
                    return true; // Port is free
                }
                finally
                {
                    tcpListener.Stop();
                }
            }
            catch (SocketException)
            {
                return false; // Port is in use
            }
        }

    }
}
