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
using MarketPlace924.DBConnection;  

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MarketPlace924.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MyCartView : Page
    {
        public ShoppingCartViewModel ViewModel { get; }

        public MyCartView()
        {
            this.InitializeComponent();
            /// <summary>
            /// Initializes a new instance of the <see cref="ShoppingCartViewModel"/> class.
            /// /// </summary>
            /// TODO change to be dynamic, but for testing, let buyerID=2
            this.ViewModel = new ShoppingCartViewModel(new Repository.ShoppingCartRepository(new DBConnection.DatabaseConnection()), buyerId: 2);
        }
    }
}
