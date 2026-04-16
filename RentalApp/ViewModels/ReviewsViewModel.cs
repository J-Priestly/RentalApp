using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Services;

namespace RentalApp.ViewModels;

[QueryProperty(nameof(ItemId), "itemId")]
[QueryProperty(nameof(RentalId), "rentalId")]
public partial class ReviewsViewModel : BaseViewModel
{
    private readonly IReviewService _reviewService;
    private readonly IReviewRepository _reviewRepository;

    [ObservableProperty]
    private int itemId;

    [ObservableProperty]
    private int rentalId;

    [ObservableProperty]
    private ObservableCollection<Review> reviews = new();

    [ObservableProperty]
    private int selectedRating = 5;

    [ObservableProperty]
    private string comment = string.Empty;

    [ObservableProperty]
    private double averageRating;

    [ObservableProperty]
    private bool canSubmitReview;

    public ReviewsViewModel(IReviewService reviewService, IReviewRepository reviewRepository)
    {
        _reviewService = reviewService;
        _reviewRepository = reviewRepository;
        Title = "Reviews";
    }

    partial void OnItemIdChanged(int value)
    {
        LoadReviewsCommand.Execute(null);
    }

    partial void OnRentalIdChanged(int value)
    {
        CanSubmitReview = value > 0;
    }

    [RelayCommand]
    private async Task LoadReviewsAsync()
    {
        if (IsBusy || ItemId == 0) return;
        IsBusy = true;
        ClearError();

        try
        {
            var (reviews, average) = await _reviewService.GetReviewsAsync(ItemId);
            Reviews = new ObservableCollection<Review>(reviews);
            AverageRating = average;
        }
        catch (Exception ex)
        {
            SetError($"Failed to load reviews: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SubmitReviewAsync()
    {
        if (IsBusy || RentalId == 0) return;
        IsBusy = true;
        ClearError();

        try
        {
            var (success, message, review) = await _reviewService.SubmitReviewAsync(
                ItemId, RentalId, SelectedRating, Comment);

            if (success && review != null)
            {
                try { await _reviewRepository.AddAsync(review); } catch { /* local DB unavailable */ }
                Comment = string.Empty;
                SelectedRating = 5;
                await LoadReviewsAsync();
            }
            else
            {
                SetError(message);
            }
        }
        catch (Exception ex)
        {
            SetError($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
