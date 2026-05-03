namespace RentalApp.Database.Models;

public class NearbyItemResult
{
    public Item Item { get; set; } = null!;
    public double DistanceMiles { get; set; }
}
