using System.Net.Http.Headers;
using System.Net.Http.Json;
using RentalApp.Database.Models;

namespace RentalApp.Services;

public class ApiService : IApiService
{
    private const string BaseUrl = "https://set09102-api.b-davison.workers.dev";
    private readonly HttpClient _http;
    private string? _token;
    private int _currentUserId;
    private string? _email;
    private string? _password;

    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);
    public int CurrentUserId => _currentUserId;

    public ApiService()
    {
        _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    private void ApplyAuth()
    {
        if (!string.IsNullOrEmpty(_token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
    }


    private async Task<bool> HandleUnauthorizedAsync()
    {
        if (_email != null && _password != null)
        {
            _token = null;
            var result = await LoginAsync(_email, _password);
            return result != null;
        }
        return false;
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
                _currentUserId = result.UserId;
                _email = email;
                _password = password;
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
            var result = await _http.GetFromJsonAsync<CategoriesResponse>("/categories");
            return result?.Categories ?? Enumerable.Empty<Category>();
        }
        catch { return Enumerable.Empty<Category>(); }
    }

    public async Task<IEnumerable<Item>> GetItemsAsync()
    {
        ApplyAuth();
        try
        {
            var response = await _http.GetAsync("/items");

           
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                    response = await _http.GetAsync("/items");
                else
                    return Enumerable.Empty<Item>();
            }

            var result = await response.Content.ReadFromJsonAsync<ItemsResponse>();
            return result?.Items ?? Enumerable.Empty<Item>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetItems error: {ex.Message}");
            return Enumerable.Empty<Item>();
        }
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

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"CreateItem failed: {response.StatusCode} - {error}");
                return null;
            }
            return await response.Content.ReadFromJsonAsync<Item>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CreateItem exception: {ex.Message}");
            return null;
        }
    }

    public async Task<Item?> UpdateItemAsync(int id, string title, string description, decimal dailyRate, int categoryId, double latitude, double longitude)
    {
        ApplyAuth();
        try
        {
            var response = await _http.PutAsJsonAsync($"/items/{id}", new
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

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"CreateRental failed: {response.StatusCode} - {error}");
                return null;
            }
            return await response.Content.ReadFromJsonAsync<Rental>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CreateRental exception: {ex.Message}");
            return null;
        }
    }

    public async Task<IEnumerable<Rental>> GetIncomingRentalsAsync()
    {
        ApplyAuth();
        try
        {
            var result = await _http.GetFromJsonAsync<RentalsResponse>("/rentals/incoming");
            return result?.Rentals ?? Enumerable.Empty<Rental>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Incoming rentals error: {ex.Message}");
            return Enumerable.Empty<Rental>();
        }
    }

    public async Task<IEnumerable<Rental>> GetOutgoingRentalsAsync()
    {
        ApplyAuth();
        try
        {
            var result = await _http.GetFromJsonAsync<RentalsResponse>("/rentals/outgoing");
            return result?.Rentals ?? Enumerable.Empty<Rental>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Outgoing rentals error: {ex.Message}");
            return Enumerable.Empty<Rental>();
        }
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