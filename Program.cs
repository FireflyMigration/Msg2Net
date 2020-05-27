using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Msg2Net
{
    class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

        [StructLayout(LayoutKind.Sequential)]
        struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;       // The count of bytes in the message.
            public IntPtr lpData;    // The address of the message.
        }

        const int WM_COPYDATA = 0x004A;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(("Usage: Msg2Net \"[Process Name]\" (parameters)"));
                return;
            }

            var proc = Process.GetProcessesByName(args[0]);
            if (proc.Length == 0)
            {
                Console.WriteLine("Process '{0}' not found", args[0]);
                return;
            }

            var currentSessionID = Process.GetCurrentProcess().SessionId;
            Process currentProcess = null;
            foreach (Process pr in proc)
            {
                if (pr.SessionId == currentSessionID)
                { 
                    currentProcess = pr;
                    break;
                }
            }
            if (currentProcess == null)
            {
                Console.WriteLine("Process '{0}' not found", args[0]);
                return;
            }
            var hwnd = currentProcess.MainWindowHandle;
            if (hwnd == IntPtr.Zero)
            {
                Console.WriteLine("Window '{0}' not found", args[0]);
                return;
            }

            var bytes = new List<byte>();
            for (var i = 1; i < args.Length; i++)
            {
                bytes.AddRange(Encoding.Default.GetBytes(args[i]));
                bytes.Add(0);
            }

            var data = new COPYDATASTRUCT();
            data.dwData = new IntPtr(args.Length - 1);
            data.cbData = bytes.Count;

            var p = Marshal.AllocHGlobal(bytes.Count);
            try
            {
                Marshal.Copy(bytes.ToArray(), 0, p, bytes.Count);
                data.lpData = p;
                SendMessage(hwnd, WM_COPYDATA, IntPtr.Zero, ref data);
            }
            finally
            {
                Marshal.FreeHGlobal(p);
            }
        }
    }
}
