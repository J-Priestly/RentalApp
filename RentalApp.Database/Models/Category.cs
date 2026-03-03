namespace RentalApp.Database.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}

public class CategoriesResponse
{
    public List<Category> Categories { get; set; } = new();
}

public class ItemsResponse
{
    public List<Item> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class RentalsResponse
{
    public List<Rental> Rentals { get; set; } = new();
    public int TotalRentals { get; set; }
}