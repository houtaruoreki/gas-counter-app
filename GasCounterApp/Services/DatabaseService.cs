using SQLite;
using GasCounterApp.Models;

namespace GasCounterApp.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _database;
    private readonly string _dbPath;

    public DatabaseService()
    {
        // Store database in external storage so it persists after clearing app data
        _dbPath = GetExternalDatabasePath();
    }

    private string GetExternalDatabasePath()
    {
        try
        {
            string externalPath;

#if ANDROID
            // Use Android's external storage Downloads directory
            // This persists even after clearing app data
            var downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(
                Android.OS.Environment.DirectoryDownloads)?.AbsolutePath;

            if (string.IsNullOrEmpty(downloadsPath))
            {
                // Fallback to external storage root + Downloads
                var externalStorage = Android.OS.Environment.ExternalStorageDirectory?.AbsolutePath;
                downloadsPath = externalStorage != null
                    ? Path.Combine(externalStorage, "Download")
                    : null;
            }

            // If we got a valid downloads path, use it
            if (!string.IsNullOrEmpty(downloadsPath))
            {
                externalPath = Path.Combine(downloadsPath, "GasCounterApp");
            }
            else
            {
                // Final fallback to app data directory
                externalPath = FileSystem.AppDataDirectory;
            }
#else
            // For other platforms, use app data directory
            externalPath = FileSystem.AppDataDirectory;
#endif

            // Ensure directory exists
            if (!Directory.Exists(externalPath))
            {
                Directory.CreateDirectory(externalPath);
            }

            var dbPath = Path.Combine(externalPath, "gascounters.db3");

            // Migrate old database if exists
            MigrateOldDatabaseIfNeeded(dbPath);

            return dbPath;
        }
        catch (Exception ex)
        {
            // If anything fails, fall back to app data directory
            Console.WriteLine($"Failed to initialize external storage database path: {ex.Message}");
            Console.WriteLine("Falling back to app data directory");

            var fallbackPath = Path.Combine(FileSystem.AppDataDirectory, "gascounters.db3");
            MigrateOldDatabaseIfNeeded(fallbackPath);
            return fallbackPath;
        }
    }

    private void MigrateOldDatabaseIfNeeded(string newPath)
    {
        var oldPath = Path.Combine(FileSystem.AppDataDirectory, "gascounters.db3");

        // If old database exists and new one doesn't, copy it
        if (File.Exists(oldPath) && !File.Exists(newPath))
        {
            try
            {
                File.Copy(oldPath, newPath, false);
                // Keep old database for safety - user can delete it manually if needed
            }
            catch
            {
                // If migration fails, continue with new path
            }
        }
    }

    private async Task InitializeAsync()
    {
        if (_database != null)
            return;

        _database = new SQLiteAsyncConnection(_dbPath);
        await _database.CreateTableAsync<GasCounter>();
    }

    public async Task<List<GasCounter>> GetAllCountersAsync()
    {
        await InitializeAsync();
        return await _database!.Table<GasCounter>().ToListAsync();
    }

    public async Task<GasCounter?> GetCounterByIdAsync(int id)
    {
        await InitializeAsync();
        return await _database!.Table<GasCounter>()
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<GasCounter>> SearchCountersByIdAsync(string counterId)
    {
        await InitializeAsync();
        return await _database!.Table<GasCounter>()
            .Where(c => c.CounterId != null && c.CounterId.Contains(counterId))
            .ToListAsync();
    }

    public async Task<bool> IsCounterIdUniqueAsync(string counterId, int excludeId = 0)
    {
        await InitializeAsync();

        if (string.IsNullOrWhiteSpace(counterId))
            return true; // Empty IDs are allowed (optional field)

        // Get all counters and do case-insensitive comparison in memory
        // SQLite's collation can be inconsistent across platforms
        var allCounters = await _database!.Table<GasCounter>()
            .Where(c => c.CounterId != null)
            .ToListAsync();

        var existing = allCounters
            .Where(c => c.Id != excludeId &&
                        string.Equals(c.CounterId, counterId, StringComparison.OrdinalIgnoreCase))
            .Count();

        return existing == 0;
    }

    public async Task<int> SaveCounterAsync(GasCounter counter)
    {
        await InitializeAsync();
        counter.ModifiedDate = DateTime.Now;

        if (counter.Id != 0)
        {
            await _database!.UpdateAsync(counter);
            return counter.Id;
        }
        else
        {
            return await _database!.InsertAsync(counter);
        }
    }

    public async Task<int> DeleteCounterAsync(GasCounter counter)
    {
        await InitializeAsync();
        return await _database!.DeleteAsync(counter);
    }

    public async Task<int> GetTotalCountersAsync()
    {
        await InitializeAsync();
        return await _database!.Table<GasCounter>().CountAsync();
    }

    public async Task<int> GetCheckedCountersAsync()
    {
        await InitializeAsync();
        return await _database!.Table<GasCounter>()
            .Where(c => c.IsChecked)
            .CountAsync();
    }

    public async Task<int> GetCountersByStateAsync(string state)
    {
        await InitializeAsync();
        return await _database!.Table<GasCounter>()
            .Where(c => c.State == state)
            .CountAsync();
    }

    public async Task<int> ResetAllChecksAsync()
    {
        await InitializeAsync();
        var allCounters = await _database!.Table<GasCounter>().ToListAsync();

        foreach (var counter in allCounters)
        {
            counter.IsChecked = false;
            counter.LastCheckedDate = null;
            counter.ModifiedDate = DateTime.Now;
        }

        return await _database!.UpdateAllAsync(allCounters);
    }

    public async Task<string> GetDatabasePathAsync()
    {
        await InitializeAsync();
        return _dbPath;
    }

    public async Task CloseDatabaseAsync()
    {
        if (_database != null)
        {
            await _database.CloseAsync();
            _database = null;
        }
    }

    public string GetDatabasePath()
    {
        return _dbPath;
    }
}
