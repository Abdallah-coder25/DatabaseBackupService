using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBackupService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
        //    ServiceBase[] ServicesToRun;
        //    ServicesToRun = new ServiceBase[]
        //    {
        //        new DataBaseBackup()
        //    };
        //    ServiceBase.Run(ServicesToRun);
            if (Environment.UserInteractive)
            {
                Console.WriteLine("Running in console mode...");
                DataBaseBackup service = new DataBaseBackup();service.StartInConsole();
            }
            else
            {
                // Running as a Windows Service
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new DataBaseBackup()
                };
               ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
