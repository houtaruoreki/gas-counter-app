using System.Linq;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Tiling;
using Mapsui.UI.Maui;
using NetTopologySuite.Geometries;
using GasCounterApp.Models;
using GasCounterApp.Services;
using MauiLocation = Microsoft.Maui.Devices.Sensors.Location;
using MauiMap = Microsoft.Maui.ApplicationModel.Map;
using MapsuiMap = Mapsui.Map;
using MapsuiBrush = Mapsui.Styles.Brush;

namespace GasCounterApp.Views;

public partial class MapPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly LocationService _locationService;
    private readonly BackupService _backupService;
    private WritableLayer? _countersLayer;
    private WritableLayer? _currentLocationLayer;
    private CancellationTokenSource? _locationUpdatesCts;
    private bool _hasAutocentered = false;

    public MapPage(DatabaseService databaseService, LocationService locationService, BackupService backupService)
    {
        InitializeComponent();

        _databaseService = databaseService;
        _locationService = locationService;
        _backupService = backupService;

        InitializeMap();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Run independent operations in parallel for faster loading
        var loadCountersTask = LoadCountersAsync();
        var updateStatsTask = UpdateStatsAsync();
        var backupCheckTask = CheckAndPerformBackupAsync();

        await Task.WhenAll(loadCountersTask, updateStatsTask, backupCheckTask);

        // Start location updates after initial load
        await StartLocationUpdatesAsync();

        // Auto-recenter to user's location when opening the app
        await RecenterToUserLocationAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopLocationUpdates();
    }

    private void InitializeMap()
    {
        var map = new MapsuiMap();

        // Add OpenStreetMap layer
        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        // Create layer for counter markers
        _countersLayer = new WritableLayer
        {
            Name = "Counters",
            Style = null
        };
        map.Layers.Add(_countersLayer);

        // Create layer for current location marker
        _currentLocationLayer = new WritableLayer
        {
            Name = "CurrentLocation",
            Style = null
        };
        map.Layers.Add(_currentLocationLayer);

        MapView.Map = map;

        // Set initial center (Ozurgeti, Georgia) after map is assigned
        var centerPoint = SphericalMercator.FromLonLat(42.0041, 41.9245);
        MapView.Map.Navigator.CenterOnAndZoomTo(centerPoint.ToMPoint(), MapView.Map.Navigator.Resolutions[14]);

        // Handle map info/tap events for features
        MapView.Info += OnMapInfo;
    }

 private async void OnMapInfo(object? sender, MapInfoEventArgs e)
{
    var map = MapView.Map;
    if (map == null || _countersLayer == null)
        return;

    try
    {
        // Get MapInfo for the counters layer only
        var mapInfo = e.GetMapInfo(new[] { (Mapsui.Layers.ILayer)_countersLayer });

        if (mapInfo?.Feature == null)
            return;

        var feature = mapInfo.Feature;

        // Check if this is a counter feature
        if (feature["CounterId"] is int counterId)
        {
            var counter = await _databaseService.GetCounterByIdAsync(counterId);
            if (counter != null)
            {
                await ShowCounterPopupAsync(counter);
            }
        }

        // Mark event as handled
        e.Handled = true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error handling map info: {ex.Message}");
    }
}

    private async Task LoadCountersAsync()
    {
        if (_countersLayer == null)
            return;

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            var counters = await _databaseService.GetAllCountersAsync();
            _countersLayer.Clear();

            foreach (var counter in counters)
            {
                AddCounterToMap(counter);
            }

            MapView.Refresh();
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private void AddCounterToMap(GasCounter counter)
    {
        if (_countersLayer == null)
            return;

        var point = SphericalMercator.FromLonLat(counter.Longitude, counter.Latitude);

        var feature = new PointFeature(point.ToMPoint());
        feature["CounterId"] = counter.Id;
        feature["IsChecked"] = counter.IsChecked;

        // Set marker style based on check status
        var color = counter.IsChecked ? Mapsui.Styles.Color.FromString("Green") : Mapsui.Styles.Color.FromString("Red");

        feature.Styles.Add(new Mapsui.Styles.SymbolStyle
        {
            SymbolScale = 0.8,
            Fill = new MapsuiBrush(color),
            Outline = new Mapsui.Styles.Pen(Mapsui.Styles.Color.White, 2)
        });

        _countersLayer.Add(feature);
    }

    private async Task StartLocationUpdatesAsync()
    {
        _locationUpdatesCts = new CancellationTokenSource();

        // Use the refactored LocationService with cancellation token
        _ = Task.Run(async () =>
        {
            await _locationService.StartListeningAsync((location, accuracy) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var accuracyText = _locationService.GetAccuracyDescription(accuracy);
                    AccuracyLabel.Text = $"GPS: {accuracyText}";

                    // Auto-center on first good GPS lock
                    if (!_hasAutocentered && accuracy.HasValue && accuracy.Value < 20)
                    {
                        _hasAutocentered = true;
                        var point = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
                        MapView.Map?.Navigator.CenterOnAndZoomTo(point.ToMPoint(), MapView.Map.Navigator.Resolutions[16]);
                        MapView.Refresh();
                    }

                    // Update current location marker
                    UpdateCurrentLocationMarker(location);
                });
            }, _locationUpdatesCts.Token);
        }, _locationUpdatesCts.Token);

        await Task.CompletedTask;
    }

    private void UpdateCurrentLocationMarker(MauiLocation location)
    {
        if (_currentLocationLayer == null)
            return;

        try
        {
            // Clear previous location marker
            _currentLocationLayer.Clear();

            // Add new current location marker
            var point = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
            var feature = new PointFeature(point.ToMPoint());

            // Style as a blue circle with white border to distinguish from counter pins
            feature.Styles.Add(new Mapsui.Styles.SymbolStyle
            {
                SymbolScale = 1.0,
                Fill = new MapsuiBrush(Mapsui.Styles.Color.FromString("Blue")),
                Outline = new Mapsui.Styles.Pen(Mapsui.Styles.Color.White, 3)
            });

            _currentLocationLayer.Add(feature);
            MapView.Refresh();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating location marker: {ex.Message}");
        }
    }

    private void StopLocationUpdates()
    {
        _locationUpdatesCts?.Cancel();
        _locationUpdatesCts?.Dispose();
        _locationUpdatesCts = null;
    }

    private async void OnMyLocationClicked(object sender, EventArgs e)
    {
        await RecenterToUserLocationAsync();
    }

    private async Task RecenterToUserLocationAsync()
    {
        var (location, _) = await _locationService.GetCurrentLocationAsync();

        if (location != null)
        {
            var point = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
            MapView.Map?.Navigator.CenterOnAndZoomTo(point.ToMPoint(), MapView.Map.Navigator.Resolutions[16]);
            MapView.Refresh();
        }
        else
        {
            await DisplayAlert("შეცდომა", "GPS მდებარეობა მიუწვდომელია", "OK");
        }
    }

    private async void OnAddCounterClicked(object sender, EventArgs e)
    {
        try
        {
            // Show loading state immediately
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            // Try to use last known location first for faster response
            var (lastLocation, lastAccuracy) = _locationService.GetLastKnownLocation();

            MauiLocation? location = lastLocation;
            double? accuracy = lastAccuracy;

            // If no recent location, get fresh GPS fix
            if (location == null)
            {
                (location, accuracy) = await _locationService.GetCurrentLocationAsync();
            }

            // Hide loading
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;

            if (location == null)
            {
                var useManual = await DisplayAlert(
                    "GPS მიუწვდომელია",
                    "GPS სიგნალი ვერ მოიძებნა. გსურთ ხელით მონიშვნა რუკაზე?",
                    "დიახ",
                    "გაუქმება");

                if (useManual)
                {
                    await DisplayAlert("ინფორმაცია", "შეეხეთ რუკას მრიცხველის დასამატებლად", "OK");
                    // TODO: Enable manual pin placement mode
                }
                return;
            }

            var addPage = new AddCounterPage(_databaseService, location.Latitude, location.Longitude, accuracy);
            addPage.CounterSaved += async (s, counter) =>
            {
                await LoadCountersAsync();
                await UpdateStatsAsync();
            };

            if (Navigation != null)
            {
                await Navigation.PushAsync(addPage);
            }
            else
            {
                await DisplayAlert("შეცდომა", "ნავიგაცია ვერ მოიძებნა", "OK");
            }
        }
        catch (Exception ex)
        {
            // Hide loading on error
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;

            await DisplayAlert("შეცდომა", $"დაფიქსირდა შეცდომა: {ex.Message}", "OK");
        }
    }

    private async void OnDashboardClicked(object sender, EventArgs e)
    {
        try
        {
            var dashboardPage = new DashboardPage(_databaseService, _backupService);
            dashboardPage.CountersUpdated += async (s, e) =>
            {
                await LoadCountersAsync();
                await UpdateStatsAsync();
            };

            if (Navigation != null)
            {
                await Navigation.PushAsync(dashboardPage);
            }
            else
            {
                await DisplayAlert("შეცდომა", "ნავიგაცია ვერ მოიძებნა", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("შეცდომა", $"დაფიქსირდა შეცდომა: {ex.Message}", "OK");
        }
    }

    private async Task ShowCounterPopupAsync(GasCounter counter)
    {
        var action = await DisplayActionSheet(
            $"ID: {counter.CounterId ?? "არ არის მითითებული"}",
            "გაუქმება",
            null,
            counter.IsChecked ? "მონიშვნის გაუქმება" : "შემოწმებულად მონიშვნა",
            "დეტალები",
            "ნავიგაცია");

        switch (action)
        {
            case "შემოწმებულად მონიშვნა":
            case "მონიშვნის გაუქმება":
                counter.IsChecked = !counter.IsChecked;
                counter.LastCheckedDate = counter.IsChecked ? DateTime.Now : null;
                await _databaseService.SaveCounterAsync(counter);
                await LoadCountersAsync();
                await UpdateStatsAsync();
                break;

            case "დეტალები":
                var detailPage = new CounterDetailPage(_databaseService, counter.Id);
                detailPage.CounterUpdated += async (s, e) =>
                {
                    await LoadCountersAsync();
                    await UpdateStatsAsync();
                };
                await Navigation.PushAsync(detailPage);
                break;

            case "ნავიგაცია":
                await OpenNavigationAsync(counter.Latitude, counter.Longitude);
                break;
        }
    }

    private async Task OpenNavigationAsync(double latitude, double longitude)
    {
        try
        {
            var location = new MauiLocation(latitude, longitude);
            var options = new MapLaunchOptions { NavigationMode = NavigationMode.Driving };
            await MauiMap.OpenAsync(location, options);
        }
        catch (Exception ex)
        {
            await DisplayAlert("შეცდომა", $"ნავიგაციის გახსნა ვერ მოხერხდა: {ex.Message}", "OK");
        }
    }

    private async Task UpdateStatsAsync()
    {
        var total = await _databaseService.GetTotalCountersAsync();
        var checked_count = await _databaseService.GetCheckedCountersAsync();

        StatsLabel.Text = $"{checked_count}/{total} შემოწმებული";
    }

    private async Task CheckAndPerformBackupAsync()
    {
        var lastBackup = await _backupService.GetLastBackupDateAsync();

        if (!lastBackup.HasValue || (DateTime.Now - lastBackup.Value).TotalHours >= 24)
        {
            await _backupService.CreateBackupAsync();
        }
    }
}
