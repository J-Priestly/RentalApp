using RentalApp.Database.Models;

namespace RentalApp.ViewModels;


// list cards can display the item name instead of a raw ID.
public class RentalDisplayItem
{
    public Rental Rental { get; init; } = null!;
    public string ItemTitle { get; init; } = string.Empty;
}
