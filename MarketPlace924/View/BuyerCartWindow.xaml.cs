// <copyright file="BuyerCartWindow.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.View
{
    using MarketPlace924.ViewModel;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// A control that displays and manages the buyer's cart.
    /// </summary>
    /// <seealso cref="ShoppingCartViewModel"/>
    public sealed partial class BuyerCartWindow : UserControl
    {
        /// <summary>
        /// Dependency property for the ViewModel associated with this control.
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(ShoppingCartViewModel), // Corrected to match the actual ViewModel type
                typeof(BuyerCartWindow),
                new PropertyMetadata(null, OnChildViewModelChanged));

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerCartWindow"/> class.
        /// </summary>
        public BuyerCartWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the ViewModel for this control.
        /// </summary>
        public ShoppingCartViewModel ViewModel // Corrected to use the correct ViewModel type
        {
            get => (ShoppingCartViewModel)this.GetValue(ViewModelProperty); // Ensure it gets the correct type
            set => this.SetValue(ViewModelProperty, value);
        }

        /// <summary>
        /// Handles changes to the ViewModel property.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">The dependency property changed event arguments.</param>
        private static void OnChildViewModelChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var control = (BuyerCartWindow)dependencyObject; // Corrected to refer to BuyerCartWindow
            control.DataContext = eventArgs.NewValue;
        }
    }
}
