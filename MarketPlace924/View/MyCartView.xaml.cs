using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MarketPlace924.ViewModel; 
using MarketPlace924.Repository;
using MarketPlace924.Service;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MarketPlace924.View
{
    /// <summary>
    /// A page that displays the shopping cart.
    /// </summary>
    public sealed partial class MyCartView : Page
    {
        public ShoppingCartViewModel ViewModel { get; set; }

        public MyCartView()
        {
            this.InitializeComponent();
            this.ViewModel = new ShoppingCartViewModel(new ShoppingCartService(), buyerId: 2);
            this.DataContext = this.ViewModel;

            // Load cart items when the page is initialized
            _ = this.ViewModel.LoadCartItemsAsync();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ShoppingCartViewModel viewModel)
            {
                this.ViewModel = viewModel;
                this.DataContext = this.ViewModel;
            }

            // Ensure cart items are loaded
            _ = this.ViewModel.LoadCartItemsAsync();
        }
    }
}
