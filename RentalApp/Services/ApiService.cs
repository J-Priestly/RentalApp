using System.Net.Http.Headers;
using System.Net.Http.Json;
using RentalApp.Database.Models;

namespace RentalApp.Services;

public class ApiService : IApiService
{
    private const string BaseUrl = "https://set09102-api.b-davison.workers.dev";
    private readonly HttpClient _http;
    private string? _token;

    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

    public ApiService()
    {
        _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    private void ApplyAuth()
    {
        if (!string.IsNullOrEmpty(_token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
    }

    public async Task<ApiTokenResponse?> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/auth/token", new { email, password });
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<ApiTokenResponse>();
            if (result != null)
            {
                _token = result.Token;
                ApplyAuth();
            }
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Login error: {ex.Message}");
            return null;
        }
    }

    public async Task<ApiRegisterResponse?> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/auth/register", new
            {
                firstName,
                lastName,
                email,
                password
            });

            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ApiRegisterResponse>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Register error: {ex.Message}");
            return null;
        }
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        ApplyAuth();
        try
        {
            return await _http.GetFromJsonAsync<IEnumerable<Category>>("/categories")
                   ?? Enumerable.Empty<Category>();
        }
        catch { return Enumerable.Empty<Category>(); }
    }

    public async Task<IEnumerable<Item>> GetItemsAsync()
    {
        ApplyAuth();
        try
        {
            return await _http.GetFromJsonAsync<IEnumerable<Item>>("/items")
                   ?? Enumerable.Empty<Item>();
        }
        catch { return Enumerable.Empty<Item>(); }
    }

    public async Task<Item?> GetItemAsync(int id)
    {
        ApplyAuth();
        try
        {
            return await _http.GetFromJsonAsync<Item>($"/items/{id}");
        }
        catch { return null; }
    }

    public async Task<Item?> CreateItemAsync(string title, string description, decimal dailyRate, int categoryId, double latitude, double longitude)
    {
        ApplyAuth();
        try
        {
            var response = await _http.PostAsJsonAsync("/items", new
            {
                title,
                description,
                dailyRate,
                categoryId,
                latitude,
                longitude
            });

            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<Item>();
        }
        catch { return null; }
    }

    public async Task<Rental?> CreateRentalAsync(int itemId, DateTime startDate, DateTime endDate)
    {
        ApplyAuth();
        try
        {
            var response = await _http.PostAsJsonAsync("/rentals", new
            {
                itemId,
                startDate = startDate.ToString("yyyy-MM-dd"),
                endDate = endDate.ToString("yyyy-MM-dd")
            });

            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<Rental>();
        }
        catch { return null; }
    }

    public async Task<IEnumerable<Rental>> GetIncomingRentalsAsync()
    {
        ApplyAuth();
        try
        {
            return await _http.GetFromJsonAsync<IEnumerable<Rental>>("/rentals/incoming")
                   ?? Enumerable.Empty<Rental>();
        }
        catch { return Enumerable.Empty<Rental>(); }
    }

    public async Task<IEnumerable<Rental>> GetOutgoingRentalsAsync()
    {
        ApplyAuth();
        try
        {
            return await _http.GetFromJsonAsync<IEnumerable<Rental>>("/rentals/outgoing")
                   ?? Enumerable.Empty<Rental>();
        }
        catch { return Enumerable.Empty<Rental>(); }
    }

    public async Task<ApiStatusResponse?> UpdateRentalStatusAsync(int id, string status)
    {
        ApplyAuth();
        try
        {
            var response = await _http.PatchAsJsonAsync($"/rentals/{id}/status", new { status });
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ApiStatusResponse>();
        }
        catch { return null; }
    }

}

