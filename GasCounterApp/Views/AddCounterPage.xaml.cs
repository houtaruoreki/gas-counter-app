using GasCounterApp.Models;
using GasCounterApp.Services;

namespace GasCounterApp.Views;

public partial class AddCounterPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly double _latitude;
    private readonly double _longitude;
    private readonly double? _accuracy;

    public event EventHandler<GasCounter>? CounterSaved;

    public AddCounterPage(double latitude, double longitude, double? accuracy)
    {
        InitializeComponent();

        _databaseService = new DatabaseService();
        _latitude = latitude;
        _longitude = longitude;
        _accuracy = accuracy;

        InitializeLocationInfo();
    }

    private void InitializeLocationInfo()
    {
        LatitudeLabel.Text = $"გრძედი: {_latitude:F6}";
        LongitudeLabel.Text = $"განედი: {_longitude:F6}";

        if (_accuracy.HasValue)
        {
            AccuracyLabel.Text = $"სიზუსტე: ±{_accuracy.Value:F1}მ";

            if (_accuracy.Value < 10)
                AccuracyLabel.TextColor = Colors.Green;
            else if (_accuracy.Value < 20)
                AccuracyLabel.TextColor = Colors.Orange;
            else
                AccuracyLabel.TextColor = Colors.Red;
        }
        else
        {
            AccuracyLabel.Text = "სიზუსტე: მიუწვდომელი";
            AccuracyLabel.TextColor = Colors.Gray;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            SaveButton.IsEnabled = false;

            var counter = new GasCounter
            {
                CounterId = string.IsNullOrWhiteSpace(CounterIdEntry.Text) ? null : CounterIdEntry.Text,
                CustomerName = string.IsNullOrWhiteSpace(CustomerNameEntry.Text) ? null : CustomerNameEntry.Text,
                StreetName = string.IsNullOrWhiteSpace(StreetNameEntry.Text) ? null : StreetNameEntry.Text,
                Latitude = _latitude,
                Longitude = _longitude,
                GpsAccuracy = _accuracy,
                State = StatePicker.SelectedIndex >= 0 ? StatePicker.Items[StatePicker.SelectedIndex] : null,
                Notes = string.IsNullOrWhiteSpace(NotesEditor.Text) ? null : NotesEditor.Text
            };

            await _databaseService.SaveCounterAsync(counter);

            CounterSaved?.Invoke(this, counter);

            await DisplayAlert("წარმატება", "მრიცხველი წარმატებით დაემატა", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("შეცდომა", $"მრიცხველის შენახვა ვერ მოხერხდა: {ex.Message}", "OK");
            SaveButton.IsEnabled = true;
        }
    }
}
