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

    public async Task StartListeningAsync(Action<Location, double?> onLocationChanged, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium, // Changed from Best to Medium for battery optimization
                Timeout = TimeSpan.FromSeconds(10)
            };

            // Start periodic location updates with cancellation support
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var location = await Geolocation.Default.GetLocationAsync(request, cancellationToken);

                    if (location != null)
                    {
                        _currentLocation = location;
                        _currentAccuracy = location.Accuracy;
                        onLocationChanged?.Invoke(location, location.Accuracy);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting location update: {ex.Message}");
                }

                // Increased from 2 seconds to 5 seconds for better battery life
                await Task.Delay(5000, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listening to location: {ex.Message}");
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
