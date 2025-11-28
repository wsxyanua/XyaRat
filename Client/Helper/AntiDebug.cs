using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Client.Helper
{
    public static class AntiDebug
    {
        [DllImport("kernel32.dll")]
        private static extern bool IsDebuggerPresent();

        [DllImport("kernel32.dll")]
        private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, 
            ref PROCESS_BASIC_INFORMATION processInformation, int processInformationLength, out int returnLength);

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_BASIC_INFORMATION
        {
            public IntPtr Reserved1;
            public IntPtr PebBaseAddress;
            public IntPtr Reserved2_0;
            public IntPtr Reserved2_1;
            public IntPtr UniqueProcessId;
            public IntPtr InheritedFromUniqueProcessId;
        }

        public static void RunAntiDebug()
        {
            if (IsDebuggerAttached())
            {
                Environment.FailFast(null);
            }

            new Thread(() => {
                while (true)
                {
                    if (IsDebuggerAttached() || CheckTimingAttack())
                    {
                        Environment.FailFast(null);
                    }
                    Thread.Sleep(5000);
                }
            }) { IsBackground = true }.Start();
        }

        public static bool IsDebuggerAttached()
        {
            try
            {
                if (Debugger.IsAttached || IsDebuggerPresent())
                    return true;

                bool isDebuggerPresent = false;
                CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref isDebuggerPresent);
                if (isDebuggerPresent)
                    return true;

                PROCESS_BASIC_INFORMATION pbi = new PROCESS_BASIC_INFORMATION();
                int returnLength;
                int status = NtQueryInformationProcess(Process.GetCurrentProcess().Handle, 7, ref pbi, 
                    Marshal.SizeOf(pbi), out returnLength);
                
                if (status == 0 && pbi.Reserved1 != IntPtr.Zero)
                    return true;
            }
            catch (Exception ex)
            {
                // Non-critical: Anti-debug check failed, assume not debugged
                Logger.Error(ex);
            }
            return false;
        }

        private static bool CheckTimingAttack()
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                Thread.Sleep(500);
                sw.Stop();
                
                if (sw.ElapsedMilliseconds > 600 || sw.ElapsedMilliseconds < 400)
                    return true;
            }
            catch (Exception ex)
            {
                // Non-critical: Timing check failed, assume not debugged
                Logger.Error(ex);
            }
            return false;
        }

        public static void HideThreadFromDebugger()
        {
            try
            {
                const int ThreadHideFromDebugger = 0x11;
                NtSetInformationThread(GetCurrentThread(), ThreadHideFromDebugger, IntPtr.Zero, 0);
            }
            catch (Exception ex)
            {
                // Non-critical: Thread hiding failed
                Logger.Error(ex);
            }
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentThread();

        [DllImport("ntdll.dll")]
        private static extern int NtSetInformationThread(IntPtr threadHandle, int threadInformationClass, 
            IntPtr threadInformation, int threadInformationLength);
    }
}
