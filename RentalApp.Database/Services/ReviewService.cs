using RentalApp.Database.Models;

namespace RentalApp.Services;

public class ReviewService : IReviewService
{
    private readonly IApiService _apiService;

    public ReviewService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public bool IsValidRating(int rating)
    {
        return rating >= 1 && rating <= 5;
    }

    public double CalculateAverageRating(IEnumerable<Review> reviews)
    {
        var list = reviews.ToList();
        if (!list.Any()) return 0;
        return list.Average(r => r.Rating);
    }

    public async Task<(bool Success, string Message, Review? Review)> SubmitReviewAsync(
        int itemId, int rentalId, int rating, string comment)
    {
        if (!IsValidRating(rating))
            return (false, "Rating must be between 1 and 5", null);

        if (string.IsNullOrWhiteSpace(comment))
            return (false, "Please write a comment", null);

        var review = await _apiService.CreateReviewAsync(itemId, rentalId, rating, comment);
        return (true, "Review submitted!", review);
    }

    public async Task<(IEnumerable<Review> Reviews, double AverageRating)> GetReviewsAsync(int itemId)
    {
        var reviews = await _apiService.GetReviewsAsync(itemId);
        var list = reviews.ToList();
        var average = CalculateAverageRating(list);
        return (list, average);
    }
}
