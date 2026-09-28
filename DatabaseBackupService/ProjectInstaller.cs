using System;
using System.ComponentModel;
using System.ServiceProcess;

namespace DatabaseBackupService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        private ServiceProcessInstaller serviceProcessInstaller;
        private ServiceInstaller serviceInstaller;
        public ProjectInstaller()
        {
            InitializeComponent();

            serviceProcessInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem 
            };

            serviceInstaller = new ServiceInstaller
            {
                ServiceName = "DatabaseBackupService", 
                DisplayName = "Database Backup Service",
                StartType = ServiceStartMode.Automatic,
                ServicesDependedOn = new string[]{"MSSQLSERVER", "RpcSs", "EventLog"}    
            };

            Installers.Add(serviceProcessInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
