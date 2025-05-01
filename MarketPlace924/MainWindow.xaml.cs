// <copyright file="MainWindow.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace MarketPlace924
{
    using System.Threading.Tasks;
    using SharedClassLibrary.Helper;
    using SharedClassLibrary.Repository;
    using SharedClassLibrary.Service;
    using MarketPlace924.View;
    using MarketPlace924.View.Admin;
    using MarketPlace924.ViewModel;
    using MarketPlace924.ViewModel.Admin;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window, IOnLoginSuccessCallback
    {
        private readonly IUserService userService;
        private readonly IBuyerService buyerService;
        private readonly ISellerService sellerService;
        private readonly IAdminService adminService;
        private readonly IAnalyticsService analyticsService;
        private readonly IShoppingCartService shoppingCartService;
        private User? user;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();

            // Initialize Database Connection and Services
            IUserRepository userRepo = new UserProxyRepository(AppConfig.GetBaseApiUrl());
            IBuyerRepository buyerRepo = new BuyerProxyRepository(AppConfig.GetBaseApiUrl());
            ISellerRepository sellerRepo = new SellerProxyRepository(AppConfig.GetBaseApiUrl());

            // Initialize Services
            this.userService = new UserService(userRepo);
            this.buyerService = new BuyerService(buyerRepo, userRepo);

            this.adminService = new AdminService(userRepo);
            this.analyticsService = new AnalyticsService(userRepo, buyerRepo);

            this.sellerService = new SellerService(sellerRepo);
            this.shoppingCartService = new ShoppingCartService();

            this.LoginFrame.Navigate(typeof(LoginView), new LoginViewModel(this.userService, this));
        }

        /// <summary>
        /// On login success.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>A task.</returns>
        public async Task OnLoginSuccess(User user)
        {
            this.LoginFrame.Visibility = Visibility.Collapsed;
            this.MenuAndStage.Visibility = Visibility.Visible;
            this.user = user;

            var myMarketButton = (Button)this.MenuAndStage.FindName("MyMarketButton");
            if (myMarketButton != null)
            {
                myMarketButton.IsEnabled = user.Role == UserRole.Buyer;
            }

            switch (user.Role)
            {
                case UserRole.Buyer:
                    await this.NavigateToBuyerProfile();
                    break;
                case UserRole.Seller:
                    this.NavigateToSellerProfile();
                    break;
                case UserRole.Admin:
                    this.NavigateToAdminProfile();
                    break;
            }
        }

        /// <summary>
        /// Navigates to the login view.
        /// </summary>
        private void NavigateToLogin()
        {
            this.Stage.Navigate(typeof(LoginView), new LoginViewModel(this.userService, this));
        }

        /// <summary>
        /// Navigates to the home view.
        /// </summary>
        private void NavigateToHome()
        {
            this.NavigateToLogin();
        }

        /// <summary>
        /// Navigates to the my market view.
        /// </summary>
        private void NavigateToMyMarket()
        {
            this.Stage.Navigate(typeof(MyMarketView), new MyMarketViewModel(this.buyerService, this.user!));
        }

        /// <summary>
        /// Navigates to the seller profile view.
        /// </summary>
        private void NavigateToSellerProfile()
        {
            this.Stage.Navigate(typeof(SellerProfileView), new SellerProfileViewModel(this.user!, this.userService, this.sellerService));
        }

        /// <summary>
        /// Navigates to the buyer profile view.
        /// </summary>
        private async Task NavigateToBuyerProfile()
        {
            IBuyerProfileViewModel buyerVm = new BuyerProfileViewModel
            {
                BuyerService = this.buyerService,
                User = this.user!,
                WishlistItemDetailsProvider = new BuyerWishlistItemDetailsProvider(),
            };
            await buyerVm.LoadBuyerProfile();
            this.Stage.Navigate(typeof(BuyerProfileView), buyerVm);
        }

        /// <summary>
        /// Navigates to the admin profile view.
        /// </summary>
        private void NavigateToAdminProfile()
        {
            this.Stage.Navigate(typeof(AdminView), new AdminViewModel(this.adminService, this.analyticsService, this.userService));
        }

        /// <summary>
        /// Navigates to the profile view.
        /// </summary>
        private async Task NavigateToProfile()
        {
            if (this.user == null)
            {
                return;
            }

            switch (this.user.Role)
            {
                case UserRole.Buyer:
                    IBuyerProfileViewModel buyerProfileVm = new BuyerProfileViewModel
                    {
                        BuyerService = this.buyerService,
                        User = this.user,
                        WishlistItemDetailsProvider = new BuyerWishlistItemDetailsProvider(),
                    };
                    await buyerProfileVm.LoadBuyerProfile();
                    this.Stage.Navigate(typeof(BuyerProfileView), buyerProfileVm);
                    break;
                case UserRole.Seller:
                    this.Stage.Navigate(typeof(SellerProfileView), new SellerProfileViewModel(this.user, this.userService, this.sellerService));
                    break;
                case UserRole.Admin:
                    this.Stage.Navigate(typeof(AdminView), new AdminViewModel(this.adminService, this.analyticsService, this.userService));
                    break;
            }
        }

        /// <summary>
        /// Navigates to the My Cart view.
        /// </summary>
        private void NavigateToMyCart()
        {
            if (this.user == null || this.user.Role != UserRole.Buyer)
            {
                return; // Ensure only buyers can access the cart
            }

            var buyerId = this.user.UserId; // Use the current user's ID as the buyer ID
            var shoppingCartViewModel = new ShoppingCartViewModel(this.shoppingCartService, buyerId);
            this.Stage.Navigate(typeof(MyCartView), shoppingCartViewModel);
        }
    }
}