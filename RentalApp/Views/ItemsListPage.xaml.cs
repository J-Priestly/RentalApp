using RentalApp.ViewModels;

namespace RentalApp.Views;

public partial class ItemsListPage : ContentPage
{
    public ItemsListPage(ItemsListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ItemsListViewModel vm)
            vm.LoadItemsCommand.Execute(null);
    }
}