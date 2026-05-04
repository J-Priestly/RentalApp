using RentalApp.Database.Models;

namespace RentalApp.Services;

public interface IReviewService
{
    bool IsValidRating(int rating);
    double CalculateAverageRating(IEnumerable<Review> reviews);
    Task<(bool Success, string Message, Review? Review)> SubmitReviewAsync(
        int itemId, int rentalId, int rating, string comment);
    Task<(IEnumerable<Review> Reviews, double AverageRating)> GetReviewsAsync(int itemId);
}
