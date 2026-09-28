using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.ServiceProcess;
using System.Timers;

namespace DatabaseBackupService
{
    public partial class DataBaseBackup : ServiceBase
    {
        private string connectionString;
        private string backupFolder;
        private string logFolder;
        private int backupIntervalMinutes;

        private string logFilePath;
        private System.Timers.Timer backupTimer; 
        private int backupInProgress = 0;
        public DataBaseBackup()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.AppSettings["ConnectionString"];
            backupFolder = ConfigurationManager.AppSettings["BackupFolder"];
            logFolder = ConfigurationManager.AppSettings["LogFolder"];
            string interval = ConfigurationManager.AppSettings["BackupIntervalMinutes"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ConfigurationErrorsException("ConnectionString is not specified.");
            }

            if (string.IsNullOrWhiteSpace(backupFolder))
            {
                throw new ConfigurationErrorsException("BackupFolder is not specified.");
            }

            if (string.IsNullOrWhiteSpace(logFolder))
            {
                throw new ConfigurationErrorsException("LogFolder is not specified.");
            }

            if (!int.TryParse(interval, out backupIntervalMinutes))
            {
                throw new ConfigurationErrorsException("BackupIntervalMinutes must be a valid number.");
            }

            if (backupIntervalMinutes <= 0)
            {
                throw new ConfigurationErrorsException( "BackupIntervalMinutes must be greater than zero.");
            }

            Directory.CreateDirectory(backupFolder);
            Directory.CreateDirectory(logFolder);

            logFilePath = Path.Combine(logFolder, "service_log.txt");
        }
        private void LogServiceEvent(string message)
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
            try
            {
                File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
            }
            catch
            {

            }
            if (Environment.UserInteractive)
            {
                Console.WriteLine(logMessage);
            }

        }
        private void PerformBackup()
        {
            if (Interlocked.Exchange(ref backupInProgress, 1) == 1)
            {
                LogServiceEvent( "Backup skipped because another backup is already running.");
                return;
            }

            try
            {
                LogServiceEvent("Starting database backup...");


                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    LogServiceEvent("SQL Server connection established.");


                    string databaseName = connection.Database;

                    if (string.IsNullOrWhiteSpace(databaseName))
                    {
                        throw new Exception( "Database name could not be determined.");
                    }

                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string backupFileName = $"Backup_{databaseName}_{timestamp}.bak";
                    string backupFilePath = Path.Combine( backupFolder, backupFileName);


                    LogServiceEvent( $"Backup file: {backupFilePath}");


                    string safeDatabaseName = databaseName.Replace("]", "]]");
                    string safeBackupPath = backupFilePath.Replace("'", "''");


                    string backupQuery = $@" BACKUP DATABASE [{safeDatabaseName}] TO DISK = N'{safeBackupPath}' WITH INIT; ";


                    using (SqlCommand command = new SqlCommand(backupQuery,connection))
                    {
                        command.CommandTimeout = 0;
                        command.ExecuteNonQuery();
                    }

                    if (File.Exists(backupFilePath))
                    {
                        FileInfo fileInfo = new FileInfo(backupFilePath);

                        LogServiceEvent( $"Database backup completed successfully.");

                        LogServiceEvent(  $"Backup file created: {backupFilePath}");

                        LogServiceEvent( $"Backup file size: {fileInfo.Length / (1024.0 * 1024.0):F2} MB");
                    }
                    else
                    {
                        throw new Exception(  "SQL backup command completed, but backup file was not found.");
                    }
                }
            }
            catch (SqlException ex)
            {
                LogServiceEvent( $"SQL backup error: {ex.Message}");

                LogServiceEvent( $"SQL error number: {ex.Number}");
            }
            catch (Exception ex)
            {
                LogServiceEvent( $"Backup error: {ex.Message}");
            }
            finally
            {
                Interlocked.Exchange( ref backupInProgress,  0);
            }
        }

        private void BackupTimer_Elapsed( object sender,ElapsedEventArgs e)
        {
            LogServiceEvent("Backup interval reached.");
        }
        protected override void OnStart(string[] args)
        {
            LogServiceEvent("Service Started.");
            LogServiceEvent( $"Backup interval: {backupIntervalMinutes} minutes.");
            LogServiceEvent( $"Backup folder: {backupFolder}");
            LogServiceEvent( $"Log folder: {logFolder}");

            ThreadPool.QueueUserWorkItem(state => { PerformBackup(); });

            double intervalMilliseconds = backupIntervalMinutes * 60 * 1000;
            backupTimer = new System.Timers.Timer(intervalMilliseconds);
            backupTimer.Elapsed += BackupTimer_Elapsed;
            backupTimer.AutoReset = true;
            backupTimer.Start();
            LogServiceEvent("Backup timer started.");
        }
        protected override void OnStop()
        {
            if (backupTimer != null)
            {
                backupTimer.Stop();
                backupTimer.Dispose();
                backupTimer = null;

                LogServiceEvent("Backup timer stopped.");
            }
            LogServiceEvent("Service Stopped.");
            LogServiceEvent("=================================");
        }
        public void StartInConsole()
        {
            OnStart(null); 
            Console.WriteLine("Press Enter to stop the service...");
            Console.ReadLine(); 
            OnStop(); 
            Console.ReadKey();

        }
    }
}
