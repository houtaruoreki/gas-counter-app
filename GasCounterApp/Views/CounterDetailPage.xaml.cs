using GasCounterApp.Models;
using GasCounterApp.Services;

namespace GasCounterApp.Views;

public partial class CounterDetailPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly int _counterId;
    private GasCounter? _counter;

    public event EventHandler? CounterUpdated;
    public event EventHandler? CounterDeleted;

    // Constructor for DI (when used through service provider)
    public CounterDetailPage(DatabaseService databaseService) : this(databaseService, 0)
    {
    }

    // Constructor with parameters (when manually created with counter ID)
    public CounterDetailPage(DatabaseService databaseService, int counterId)
    {
        InitializeComponent();

        _databaseService = databaseService;
        _counterId = counterId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCounterAsync();
    }

    private async Task LoadCounterAsync()
    {
        _counter = await _databaseService.GetCounterByIdAsync(_counterId);

        if (_counter == null)
        {
            await DisplayAlert("შეცდომა", "მრიცხველი ვერ მოიძებნა", "OK");
            await Navigation.PopAsync();
            return;
        }

        // Location info
        LatitudeLabel.Text = $"გრძედი: {_counter.Latitude:F6}";
        LongitudeLabel.Text = $"განედი: {_counter.Longitude:F6}";
        AccuracyLabel.Text = _counter.GpsAccuracy.HasValue
            ? $"სიზუსტე: ±{_counter.GpsAccuracy.Value:F1}მ"
            : "სიზუსტე: მიუწვდომელი";

        // Check status
        CheckedSwitch.IsToggled = _counter.IsChecked;
        UpdateCheckStatus();

        // Fields
        CounterIdEntry.Text = _counter.CounterId;
        CustomerNameEntry.Text = _counter.CustomerName;
        StreetNameEntry.Text = _counter.StreetName;
        NotesEditor.Text = _counter.Notes;

        // State
        if (!string.IsNullOrEmpty(_counter.State))
        {
            var index = StatePicker.Items.IndexOf(_counter.State);
            if (index >= 0)
                StatePicker.SelectedIndex = index;
        }

        // Timestamps
        CreatedLabel.Text = $"შექმნილია: {_counter.CreatedDate:dd/MM/yyyy HH:mm}";
        ModifiedLabel.Text = $"შეცვლილია: {_counter.ModifiedDate:dd/MM/yyyy HH:mm}";
    }

    private void OnCheckedToggled(object sender, ToggledEventArgs e)
    {
        if (_counter != null)
        {
            _counter.IsChecked = e.Value;
            _counter.LastCheckedDate = e.Value ? DateTime.Now : null;
            UpdateCheckStatus();
        }
    }

    private void UpdateCheckStatus()
    {
        if (_counter == null)
            return;

        CheckStatusLabel.Text = _counter.IsChecked ? "შემოწმებული" : "არ არის შემოწმებული";
        CheckStatusLabel.TextColor = _counter.IsChecked ? Colors.Green : Colors.Red;

        if (_counter.IsChecked && _counter.LastCheckedDate.HasValue)
        {
            LastCheckedLabel.Text = $"შემოწმების თარიღი: {_counter.LastCheckedDate.Value:dd/MM/yyyy HH:mm}";
            LastCheckedLabel.IsVisible = true;
        }
        else
        {
            LastCheckedLabel.IsVisible = false;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_counter == null)
            return;

        try
        {
            SaveButton.IsEnabled = false;

            var counterId = string.IsNullOrWhiteSpace(CounterIdEntry.Text) ? null : CounterIdEntry.Text.Trim();

            // Check for unique Counter ID if provided and changed (case-insensitive)
            if (!string.IsNullOrWhiteSpace(counterId) &&
                !string.Equals(counterId, _counter.CounterId, StringComparison.OrdinalIgnoreCase))
            {
                var isUnique = await _databaseService.IsCounterIdUniqueAsync(counterId, _counter.Id);
                if (!isUnique)
                {
                    await DisplayAlert(
                        "შეცდომა",
                        $"მრიცხველის ID '{counterId}' უკვე არსებობს ბაზაში!\n\nგთხოვთ გამოიყენოთ უნიკალური ID.",
                        "OK");
                    SaveButton.IsEnabled = true;
                    CounterIdEntry.Focus(); // Focus back on the ID field
                    return;
                }
            }

            _counter.CounterId = counterId;
            _counter.CustomerName = string.IsNullOrWhiteSpace(CustomerNameEntry.Text) ? null : CustomerNameEntry.Text.Trim();
            _counter.StreetName = string.IsNullOrWhiteSpace(StreetNameEntry.Text) ? null : StreetNameEntry.Text.Trim();
            _counter.State = StatePicker.SelectedIndex >= 0 ? StatePicker.Items[StatePicker.SelectedIndex] : null;
            _counter.Notes = string.IsNullOrWhiteSpace(NotesEditor.Text) ? null : NotesEditor.Text.Trim();

            await _databaseService.SaveCounterAsync(_counter);

            CounterUpdated?.Invoke(this, EventArgs.Empty);

            await DisplayAlert("წარმატება", "ცვლილებები შენახულია", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("შეცდომა", $"შენახვა ვერ მოხერხდა: {ex.Message}", "OK");
            SaveButton.IsEnabled = true;
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (_counter == null)
            return;

        // Two-step deletion confirmation
        var confirm1 = await DisplayAlert(
            "წაშლის დადასტურება",
            "ნამდვილად გსურთ ამ მრიცხველის წაშლა?",
            "დიახ",
            "გაუქმება");

        if (!confirm1)
            return;

        var confirm2 = await DisplayAlert(
            "საბოლოო დადასტურება",
            "ეს მოქმედება შეუქცევადია. გაგრძელება?",
            "წაშლა",
            "გაუქმება");

        if (!confirm2)
            return;

        try
        {
            DeleteButton.IsEnabled = false;

            await _databaseService.DeleteCounterAsync(_counter);

            CounterDeleted?.Invoke(this, EventArgs.Empty);
            CounterUpdated?.Invoke(this, EventArgs.Empty);

            await DisplayAlert("წარმატება", "მრიცხველი წაშლილია", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("შეცდომა", $"წაშლა ვერ მოხერხდა: {ex.Message}", "OK");
            DeleteButton.IsEnabled = true;
        }
    }
}
