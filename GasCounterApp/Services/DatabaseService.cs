using SQLite;
using GasCounterApp.Models;

namespace GasCounterApp.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _database;
    private readonly string _dbPath;

    public DatabaseService()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "gascounters.db3");
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
