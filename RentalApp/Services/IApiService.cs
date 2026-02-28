using RentalApp.Database.Models;

namespace RentalApp.Services;

public interface IApiService
{
    bool IsAuthenticated { get; }

    // Auth
    Task<ApiTokenResponse?> LoginAsync(string email, string password);
    Task<ApiRegisterResponse?> RegisterAsync(string firstName, string lastName, string email, string password);


    // Categories
    Task<IEnumerable<Category>> GetCategoriesAsync();


    // Items
    Task<IEnumerable<Item>> GetItemsAsync();
    Task<Item?> GetItemAsync(int id);
    Task<Item?> CreateItemAsync(string title, string description, decimal dailyRate, int categoryId, double latitude, double longitude);


    // Rentals
    Task<Rental?> CreateRentalAsync(int itemId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Rental>> GetIncomingRentalsAsync();
    Task<IEnumerable<Rental>> GetOutgoingRentalsAsync();
    Task<ApiStatusResponse?> UpdateRentalStatusAsync(int id, string status);


}

public class ApiTokenResponse
{
    public string Token { get; set; } = string.Empty;
    public string ExpiresAt { get; set; } = string.Empty;
    public int UserId { get; set; }
}

public class ApiRegisterResponse
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}

public class ApiStatusResponse
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}