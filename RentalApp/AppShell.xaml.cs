using RentalApp.ViewModels;

namespace RentalApp;

public partial class AppShell : Shell
{
    public AppShell(AppShellViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();

        Routing.RegisterRoute("MainPage",       typeof(Views.MainPage));
        Routing.RegisterRoute("LoginPage",      typeof(Views.LoginPage));
        Routing.RegisterRoute("RegisterPage",   typeof(Views.RegisterPage));
        Routing.RegisterRoute("ItemsListPage",  typeof(Views.ItemsListPage));
        Routing.RegisterRoute("CreateItemPage", typeof(Views.CreateItemPage));
        Routing.RegisterRoute("ItemDetailPage", typeof(Views.ItemDetailPage));
        Routing.RegisterRoute("NearbyItemsPage",typeof(Views.NearbyItemsPage));
        Routing.RegisterRoute("RentalsPage",    typeof(Views.RentalsPage));
        Routing.RegisterRoute("ReviewsPage",    typeof(Views.ReviewsPage));
        Routing.RegisterRoute("UserListPage",   typeof(Views.UserListPage));
        Routing.RegisterRoute("UserDetailPage", typeof(Views.UserDetailPage));
        Routing.RegisterRoute("ProfilePage",    typeof(Views.ProfilePage));
    }
}
