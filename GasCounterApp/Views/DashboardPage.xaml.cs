using GasCounterApp.Models;
using GasCounterApp.Services;
using System.Collections.ObjectModel;

namespace GasCounterApp.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly BackupService _backupService;
    private ObservableCollection<GasCounter> _searchResults;

    public event EventHandler? CountersUpdated;

    public DashboardPage(DatabaseService databaseService, BackupService backupService)
    {
        InitializeComponent();

        _databaseService = databaseService;
        _backupService = backupService;
        _searchResults = new ObservableCollection<GasCounter>();

        SearchResultsView.ItemsSource = _searchResults;

        // Add value converter for checkmark
        Resources.Add("CheckmarkConverter", new CheckmarkConverter());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await UpdateStatisticsAsync();
        await UpdateBackupInfoAsync();
    }

    private async Task UpdateStatisticsAsync()
    {
        var total = await _databaseService.GetTotalCountersAsync();
        var checked_count = await _databaseService.GetCheckedCountersAsync();
        var remaining = total - checked_count;

        TotalCountersLabel.Text = total.ToString();
        CheckedCountersLabel.Text = checked_count.ToString();
        RemainingCountersLabel.Text = remaining.ToString();

        CycleStatusLabel.Text = $"მიმდინარე ციკლი: {checked_count}/{total} შემოწმებული";

        // Auto-prompt if all counters are checked
        if (total > 0 && checked_count == total)
        {
            var startNew = await DisplayAlert(
                "ყველა მრიცხველი შემოწმებულია!",
                "გსურთ ახალი ციკლის დაწყება?",
                "დიახ",
                "არა");

            if (startNew)
            {
                await ResetCycleAsync();
            }
        }
    }

    private async Task UpdateBackupInfoAsync()
    {
        var lastBackup = await _backupService.GetLastBackupDateAsync();

        if (lastBackup.HasValue)
        {
            LastBackupLabel.Text = $"ბოლო კოპია: {lastBackup.Value:dd/MM/yyyy HH:mm}";
        }
        else
        {
            LastBackupLabel.Text = "ბოლო კოპია: არასოდეს";
        }
    }

    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            var searchText = e.NewTextValue;

            // Ensure UI updates happen on main thread
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                _searchResults.Clear();

                if (string.IsNullOrWhiteSpace(searchText))
                    return;

                var results = await _databaseService.SearchCountersByIdAsync(searchText);

                foreach (var counter in results)
                {
                    _searchResults.Add(counter);
                }
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("შეცდომა", $"ძებნის შეცდომა: {ex.Message}", "OK");
        }
    }

    private async void OnSearchResultSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is GasCounter counter)
        {
            var detailPage = new CounterDetailPage(_databaseService, counter.Id);
            detailPage.CounterUpdated += (s, e) =>
            {
                CountersUpdated?.Invoke(this, EventArgs.Empty);
                MainThread.BeginInvokeOnMainThread(async () => await UpdateStatisticsAsync());
            };

            await Navigation.PushAsync(detailPage);

            // Clear selection
            SearchResultsView.SelectedItem = null;
        }
    }

    private async void OnResetCycleClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "ციკლის დაწყება",
            "ყველა მრიცხველი მონიშნული იქნება როგორც არაშემოწმებული. გაგრძელება?",
            "დიახ",
            "გაუქმება");

        if (!confirm)
            return;

        await ResetCycleAsync();
    }

    private async Task ResetCycleAsync()
    {
        try
        {
            ResetCycleButton.IsEnabled = false;

            await _databaseService.ResetAllChecksAsync();

            CountersUpdated?.Invoke(this, EventArgs.Empty);

            await DisplayAlert("წარმატება", "ახალი ციკლი დაიწყო", "OK");
            await UpdateStatisticsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("შეცდომა", $"ციკლის განულება ვერ მოხერხდა: {ex.Message}", "OK");
        }
        finally
        {
            ResetCycleButton.IsEnabled = true;
        }
    }

    private async void OnCreateBackupClicked(object sender, EventArgs e)
    {
        try
        {
            CreateBackupButton.IsEnabled = false;

            var success = await _backupService.CreateBackupAsync();

            if (success)
            {
                await DisplayAlert("წარმატება", "სარეზერვო კოპია შექმნილია", "OK");
                await UpdateBackupInfoAsync();
            }
            else
            {
                await DisplayAlert("შეცდომა", "კოპირება ვერ მოხერხდა", "OK");
            }
        }
        finally
        {
            CreateBackupButton.IsEnabled = true;
        }
    }

    private async void OnViewBackupsClicked(object sender, EventArgs e)
    {
        var backups = await _backupService.GetAvailableBackupsAsync();

        if (backups.Count == 0)
        {
            await DisplayAlert("ინფორმაცია", "სარეზერვო კოპიები არ არსებობს", "OK");
            return;
        }

        var action = await DisplayActionSheet(
            "აირჩიეთ სარეზერვო კოპია აღსადგენად",
            "გაუქმება",
            null,
            backups.ToArray());

        if (action != null && action != "გაუქმება")
        {
            var confirm = await DisplayAlert(
                "კოპიის აღდგენა",
                $"გსურთ '{action}' კოპიის აღდგენა? მიმდინარე მონაცემები შეიცვლება.",
                "დიახ",
                "გაუქმება");

            if (confirm)
            {
                var success = await _backupService.RestoreBackupAsync(action);

                if (success)
                {
                    await DisplayAlert("წარმატება", "კოპია აღდგენილია. გთხოვთ გადატვირთოთ აპლიკაცია", "OK");
                    CountersUpdated?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    await DisplayAlert("შეცდომა", "კოპიის აღდგენა ვერ მოხერხდა", "OK");
                }
            }
        }
    }
}

// Value converter for checkmark display
public class CheckmarkConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool isChecked)
        {
            return isChecked ? "✓" : "✗";
        }
        return "✗";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
