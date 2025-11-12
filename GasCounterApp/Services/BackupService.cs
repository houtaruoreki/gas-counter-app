namespace GasCounterApp.Services;

public class BackupService
{
    private readonly DatabaseService _databaseService;
    private readonly string _backupDirectory;

    public BackupService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        _backupDirectory = Path.Combine(FileSystem.AppDataDirectory, "Backups");

        // Ensure backup directory exists
        if (!Directory.Exists(_backupDirectory))
        {
            Directory.CreateDirectory(_backupDirectory);
        }
    }

    public async Task<bool> CreateBackupAsync()
    {
        try
        {
            // Close the database connection to unlock the file
            await _databaseService.CloseDatabaseAsync();

            var dbPath = _databaseService.GetDatabasePath();

            if (!File.Exists(dbPath))
            {
                // Reinitialize database
                await _databaseService.GetAllCountersAsync();
                return false;
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"gascounters_backup_{timestamp}.db3";
            var backupPath = Path.Combine(_backupDirectory, backupFileName);

            // Wait a moment for file handles to release
            await Task.Delay(100);

            File.Copy(dbPath, backupPath, true);

            // Keep only last 7 backups
            await CleanOldBackupsAsync(7);

            // Reinitialize database connection
            await _databaseService.GetAllCountersAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Backup failed: {ex.Message}");
            // Ensure database is reinitialized even on error
            try
            {
                await _databaseService.GetAllCountersAsync();
            }
            catch { }
            return false;
        }
    }

    public async Task<bool> RestoreBackupAsync(string backupFileName)
    {
        try
        {
            var backupPath = Path.Combine(_backupDirectory, backupFileName);

            if (!File.Exists(backupPath))
                return false;

            // Close the database connection to unlock the file
            await _databaseService.CloseDatabaseAsync();

            var dbPath = _databaseService.GetDatabasePath();

            // Create a temporary backup of current database
            var tempBackupPath = dbPath + ".temp";
            if (File.Exists(dbPath))
            {
                File.Copy(dbPath, tempBackupPath, true);
            }

            try
            {
                // Wait for file handles to release
                await Task.Delay(100);

                File.Copy(backupPath, dbPath, true);

                // Delete temporary backup on success
                if (File.Exists(tempBackupPath))
                {
                    File.Delete(tempBackupPath);
                }

                // Reinitialize database connection
                await _databaseService.GetAllCountersAsync();

                return true;
            }
            catch
            {
                // Restore from temporary backup if restore failed
                if (File.Exists(tempBackupPath))
                {
                    File.Copy(tempBackupPath, dbPath, true);
                    File.Delete(tempBackupPath);
                }

                // Reinitialize database
                try
                {
                    await _databaseService.GetAllCountersAsync();
                }
                catch { }

                throw;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Restore failed: {ex.Message}");
            return false;
        }
    }

    public async Task<List<string>> GetAvailableBackupsAsync()
    {
        return await Task.Run(() =>
        {
            if (!Directory.Exists(_backupDirectory))
                return new List<string>();

            return Directory.GetFiles(_backupDirectory, "*.db3")
                .Select(Path.GetFileName)
                .Where(f => f != null)
                .Cast<string>()
                .OrderByDescending(f => f)
                .ToList();
        });
    }

    private async Task CleanOldBackupsAsync(int keepCount)
    {
        await Task.Run(() =>
        {
            var backups = Directory.GetFiles(_backupDirectory, "*.db3")
                .OrderByDescending(f => File.GetCreationTime(f))
                .Skip(keepCount);

            foreach (var backup in backups)
            {
                try
                {
                    File.Delete(backup);
                }
                catch
                {
                    // Ignore deletion errors
                }
            }
        });
    }

    public async Task<DateTime?> GetLastBackupDateAsync()
    {
        return await Task.Run<DateTime?>(() =>
        {
            if (!Directory.Exists(_backupDirectory))
                return null;

            var latestBackup = Directory.GetFiles(_backupDirectory, "*.db3")
                .OrderByDescending(f => File.GetCreationTime(f))
                .FirstOrDefault();

            return latestBackup != null ? (DateTime?)File.GetCreationTime(latestBackup) : null;
        });
    }
}
