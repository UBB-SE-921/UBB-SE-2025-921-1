namespace MarketPlace924.View
{
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using MarketPlace924.ViewModel;

    /// <summary>
    /// A control that displays and manages a single item in the buyer's cart.
    /// </summary>
    public sealed partial class BuyerCartItemControl : UserControl
    {
        /// <summary>
        /// Dependency property for the ViewModel associated with this control.
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(CartItemViewModel),
                typeof(BuyerCartItemControl),
                new PropertyMetadata(null));

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerCartItemControl"/> class.
        /// </summary>
        public BuyerCartItemControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the ViewModel for this control.
        /// </summary>
        public CartItemViewModel ViewModel
        {
            get => (CartItemViewModel)this.GetValue(ViewModelProperty);
            set => this.SetValue(ViewModelProperty, value);
        }
    }
}
