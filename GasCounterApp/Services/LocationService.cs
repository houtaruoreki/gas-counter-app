namespace GasCounterApp.Services;

public class LocationService
{
    private Location? _currentLocation;
    private double? _currentAccuracy;

    public async Task<(Location? location, double? accuracy)> GetCurrentLocationAsync()
    {
        try
        {
            // Check and request location permission
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    return (null, null);
                }
            }

            var request = new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Best,
                Timeout = TimeSpan.FromSeconds(30)
            };

            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location != null)
            {
                _currentLocation = location;
                _currentAccuracy = location.Accuracy;
                return (location, location.Accuracy);
            }

            return (null, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to get location: {ex.Message}");
            return (null, null);
        }
    }

    public async Task<bool> StartListeningAsync(Action<Location, double?> onLocationChanged)
    {
        try
        {
            var request = new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Best,
                Timeout = TimeSpan.FromSeconds(10)
            };

            // Start periodic location updates
            while (true)
            {
                var location = await Geolocation.Default.GetLocationAsync(request);

                if (location != null)
                {
                    _currentLocation = location;
                    _currentAccuracy = location.Accuracy;
                    onLocationChanged?.Invoke(location, location.Accuracy);
                }

                await Task.Delay(2000); // Update every 2 seconds
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listening to location: {ex.Message}");
            return false;
        }
    }

    public (Location? location, double? accuracy) GetLastKnownLocation()
    {
        return (_currentLocation, _currentAccuracy);
    }

    public string GetAccuracyDescription(double? accuracy)
    {
        if (!accuracy.HasValue)
            return "მიუწვდომელი"; // Unavailable

        if (accuracy.Value < 5)
            return $"ძალიან ზუსტი ({accuracy.Value:F1}მ)"; // Very accurate

        if (accuracy.Value < 10)
            return $"ზუსტი ({accuracy.Value:F1}მ)"; // Accurate

        if (accuracy.Value < 20)
            return $"საშუალო ({accuracy.Value:F1}მ)"; // Medium

        return $"დაბალი ({accuracy.Value:F1}მ)"; // Low
    }

    public async Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2)
    {
        var location1 = new Location(lat1, lon1);
        var location2 = new Location(lat2, lon2);

        return await Task.Run(() => location1.CalculateDistance(location2, DistanceUnits.Kilometers) * 1000); // Return in meters
    }
}
