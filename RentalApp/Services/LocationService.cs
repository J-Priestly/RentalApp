namespace RentalApp.Services;

public class LocationService : ILocationService
{
    public async Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                return null;

            var location = await Geolocation.Default.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));

            if (location != null)
                return (location.Latitude, location.Longitude);

            return null;
        }
        catch (FeatureNotSupportedException) { return null; }
        catch (PermissionException) { return null; }
        catch { return null; }
    }
}
