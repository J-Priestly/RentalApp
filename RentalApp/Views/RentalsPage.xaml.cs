using RentalApp.ViewModels;

namespace RentalApp.Views;

public partial class RentalsPage : ContentPage
{
    public RentalsPage(RentalsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is RentalsViewModel vm)
            vm.LoadRentalsCommand.Execute(null);
    }
}