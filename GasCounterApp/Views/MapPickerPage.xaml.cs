using Mapsui;
using Mapsui.Projections;
using Mapsui.Tiling;
using Mapsui.UI.Maui;

namespace GasCounterApp.Views;

public partial class MapPickerPage : ContentPage
{
    private double _selectedLatitude;
    private double _selectedLongitude;

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
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        // Center map on initial location
        var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(_selectedLongitude, _selectedLatitude).ToMPoint();
        map.Home = n => n.CenterOn(sphericalMercatorCoordinate);

        // Set initial zoom level
        map.Navigator.ZoomTo(5000);

        MapView.Map = map;

        // Subscribe to map position changes
        MapView.Map.Navigator.Navigated += (s, e) =>
        {
            UpdateCenterCoordinates();
        };

        // Initial coordinates update
        UpdateCenterCoordinates();
    }

    private void UpdateCenterCoordinates()
    {
        // Get the center of the map viewport
        var center = MapView.Map.Navigator.Viewport.Center;

        // Convert from Spherical Mercator to WGS84
        var lonLat = SphericalMercator.ToLonLat(center.X, center.Y);

        _selectedLongitude = lonLat.X;
        _selectedLatitude = lonLat.Y;

        // Update the label
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CoordinatesLabel.Text = $"გრძედი: {_selectedLatitude:F6}, განედი: {_selectedLongitude:F6}";
        });
    }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        // Invoke the event with selected coordinates
        LocationSelected?.Invoke(this, (_selectedLatitude, _selectedLongitude));

        // Navigate back
        await Navigation.PopAsync();
    }
}
