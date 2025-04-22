// <copyright file="BuyerProfileView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.View
{
    using MarketPlace924.ViewModel;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;

    /// <summary>
    /// A view that displays and manages the buyer's profile information.
    /// </summary>
    /// <seealso cref="BuyerProfileViewModel"/>
    public sealed partial class BuyerProfileView : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerProfileView"/> class.
        /// </summary>
        public BuyerProfileView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the ViewModel for this view.
        /// </summary>
        public IBuyerProfileViewModel? ViewModel { get; set; }

        /// <summary>
        /// Called when the page is navigated to.
        /// </summary>
        /// <param name="eventArgs">The navigation event arguments.</param>
        protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
        {
            base.OnNavigatedTo(eventArgs);
            if (eventArgs.Parameter is IBuyerProfileViewModel viewModel)
            {
                this.ViewModel = viewModel;
            }
        }


    }
}