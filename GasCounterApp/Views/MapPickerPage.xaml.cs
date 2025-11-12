using Mapsui;
using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.Tiling;
using Mapsui.UI.Maui;
using MapsuiMap = Mapsui.Map;

namespace GasCounterApp.Views;

public partial class MapPickerPage : ContentPage
{
    private double _selectedLatitude;
    private double _selectedLongitude;
    private System.Timers.Timer? _updateTimer;

    public event EventHandler<(double Latitude, double Longitude)>? LocationSelected;

    public MapPickerPage(double initialLatitude, double initialLongitude)
    {
        InitializeComponent();

        _selectedLatitude = initialLatitude;
        _selectedLongitude = initialLongitude;

        InitializeMap();
    }

    private void InitializeMap()
    {
        // Create map with OpenStreetMap tiles
        var map = new MapsuiMap();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        // Center map on initial location
        var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(_selectedLongitude, _selectedLatitude);
        map.Navigator.CenterOnAndZoomTo(sphericalMercatorCoordinate.ToMPoint(), map.Navigator.Resolutions[14]);

        MapView.Map = map;

        // Use a timer to periodically update coordinates as user pans
        _updateTimer = new System.Timers.Timer(200); // Update every 200ms
        _updateTimer.Elapsed += (s, e) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateCenterCoordinates();
            });
        };
        _updateTimer.Start();

        // Initial coordinates update
        UpdateCenterCoordinates();
    }

    private void UpdateCenterCoordinates()
    {
        if (MapView?.Map?.Navigator?.Viewport == null)
            return;

        // Get the center of the map viewport
        var viewport = MapView.Map.Navigator.Viewport;
        var centerX = viewport.CenterX;
        var centerY = viewport.CenterY;

        // Convert from Spherical Mercator to WGS84
        var (lon, lat) = SphericalMercator.ToLonLat(centerX, centerY);

        _selectedLongitude = lon;
        _selectedLatitude = lat;

        // Update the label
        CoordinatesLabel.Text = $"გრძედი: {_selectedLatitude:F6}, განედი: {_selectedLongitude:F6}";
    }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        // Stop the timer
        _updateTimer?.Stop();
        _updateTimer?.Dispose();

        // Invoke the event with selected coordinates
        LocationSelected?.Invoke(this, (_selectedLatitude, _selectedLongitude));

        // Navigate back
        await Navigation.PopAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Clean up timer
        _updateTimer?.Stop();
        _updateTimer?.Dispose();
    }
}
