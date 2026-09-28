# Database Backup Service

A C# Windows Service for creating automated full backups of a SQL Server database.

The application connects to SQL Server, creates a `.bak` backup file, stores it in a configured folder, and writes detailed logs about the backup process.

## Features

- Full SQL Server database backup.
- Timestamped `.bak` files.
- Configurable SQL Server connection string.
- Configurable backup directory.
- Configurable log directory.
- Detailed logging.
- Console mode for development and testing.
- Windows Service support.
- Error handling for SQL Server connection and backup failures.
- Support for large database backups.
- Prevents multiple backups from running at the same time.
- Backup files can be restored using SQL Server Management Studio (SSMS).

## Technologies

- C#
- .NET Framework 4.7.2
- SQL Server
- SQL Server Management Studio (SSMS)
- Windows Service
- System.Data.SqlClient

## Project Structure

```text
DatabaseBackupService
│
├── DataBaseBackup.cs
│   ├── SQL Server connection
│   ├── Database backup
│   ├── Logging
│   ├── Service Start/Stop
│   └── Console Mode
│
├── Program.cs
│   ├── Windows Service mode
│   └── Console mode
│
├── ProjectInstaller.cs
│   └── Windows Service installation configuration
│
└── App.config
    ├── ConnectionString
    ├── BackupFolder
    ├── LogFolder
    └── BackupIntervalMinutes
```

## Configuration

The application uses `App.config`.

Example:

```xml
<appSettings>

    <add
        key="ConnectionString"
        value="Server=.;Database=PosSystem;Integrated Security=True;" />

    <add
        key="BackupFolder"
        value="C:\DatabaseBackups" />

    <add
        key="LogFolder"
        value="C:\DatabaseBackups\Logs" />

    <add
        key="BackupIntervalMinutes"
        value="60" />

</appSettings>
```

### Configuration Options

| Key | Description |
|---|---|
| `ConnectionString` | SQL Server connection string |
| `BackupFolder` | Folder where `.bak` files are stored |
| `LogFolder` | Folder where log files are stored |
| `BackupIntervalMinutes` | Backup interval in minutes |

## Backup File

A successful backup creates a file similar to:

```text
C:\DatabaseBackups\Backup_PosSystem_20260928_192759.bak
```

The filename contains:

- Database name
- Date
- Time

Example:

```text
Backup_PosSystem_20260928_192759.bak
```

## Logging

The service creates:

```text
C:\DatabaseBackups\Logs\service_log.txt
```

Example log:

```text
[2026-09-28 19:27:59] Database Backup Service Started.
[2026-09-28 19:27:59] Starting database backup...
[2026-09-28 19:27:59] SQL Server connection established.
[2026-09-28 19:27:59] Backup file: C:\DatabaseBackups\Backup_PosSystem_20260928_192759.bak
[2026-09-28 19:28:00] Database backup completed successfully.
[2026-09-28 19:28:00] Backup file size: 5.55 MB
```

## Console Mode

During development, the application can run as a normal console application.

The application checks:

```csharp
Environment.UserInteractive
```

If the application is running interactively, it starts in Console Mode.

Otherwise, it runs as a Windows Service.

Console Mode makes it easier to test the backup before installing the Windows Service.

## Restoring a Backup

The `.bak` file should not be opened using a normal application.

To restore the database:

1. Open SQL Server Management Studio.
2. Right-click `Databases`.
3. Select `Restore Database...`.
4. Select `Device`.
5. Select the `.bak` file.
6. Choose the destination database.
7. For testing, use a different database name such as:

```text
PosSystem_TestRestore
```

This allows the original database to remain untouched.

## Important SQL Server Permissions

The SQL Server service account must have permission to write to the backup directory.

For example:

```text
C:\DatabaseBackups
```

The C# application requesting the backup and SQL Server writing the `.bak` file are two different operations.

SQL Server itself performs:

```sql
BACKUP DATABASE
```

and writes the backup file.

## Error Handling

The application handles:

- SQL Server connection errors.
- SQL backup errors.
- Invalid configuration.
- Missing backup directory.
- Missing log directory.
- Backup file creation errors.

Errors are written to:

```text
service_log.txt
```

## Testing

Recommended testing sequence:

### 1. Successful Backup

Run the application in Console Mode and verify that a `.bak` file is created.

### 2. Backup File

Verify that the backup exists:

```text
C:\DatabaseBackups
```

### 3. Restore Test

Restore the backup as:

```text
PosSystem_TestRestore
```

### 4. Invalid Connection

Change the connection string to an invalid SQL Server and verify that the error is logged.

### 5. SQL Server Offline

Stop SQL Server and verify that the backup failure is logged.

## Windows Service

The service is configured with:

```text
Service Name:
DatabaseBackupService
```

Display name:

```text
Database Backup Service
```

Startup type:

```text
Automatic
```

The service can be installed using the Windows Service installation mechanism.

## Backup Architecture

Current architecture:

```text
Windows Service
       │
       ▼
   OnStart()
       │
       ├── PerformBackup()
       │
       ▼
   Start Timer
       │
       ▼
Every X Minutes
       │
       ▼
PerformBackup()
```

## Future Improvements

Possible future improvements include:

- Windows Task Scheduler integration.
- Weekly/daily scheduled backups.
- Configurable backup days and times.
- Backup retention policy.
- Automatic deletion of old backups.
- Backup compression.
- Backup verification.
- Email notifications.
- Event Viewer logging.
- Multiple database support.
- Separate backup configuration for each database.

## Author

C# / SQL Server learning project focused on Windows Services, SQL Server backup automation, logging, and database recovery.
