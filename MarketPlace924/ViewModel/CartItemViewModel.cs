using System.ComponentModel;
using System;
using System.Windows.Input;
using MarketPlace924.Helper;

namespace MarketPlace924.ViewModel
{
    public class CartItemViewModel : ICartItemViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string productName;
        private decimal price;
        private int quantity;
        private string imageSource;

        public CartItemViewModel()
        {
            IncreaseQuantityCommand = new RelayCommand(_ => IncreaseQuantity());
            DecreaseQuantityCommand = new RelayCommand(_ => DecreaseQuantity());
            RemoveItemCommand = new RelayCommand(_ => RemoveItem());

            // Set some default values
            ProductName = "Default Product";
            Price = 0.00m;
            Quantity = 1;
            ImageSource = "ms-appx:///Assets/placeholder.png"; // or your actual path
        }

        public string ProductName
        {
            get => productName;
            set
            {
                productName = value;
                OnPropertyChanged(nameof(ProductName));
                OnPropertyChanged(nameof(Title)); // since Title returns ProductName
            }
        }

        public decimal Price
        {
            get => price;
            set
            {
                price = value;
                OnPropertyChanged(nameof(Price));
            }
        }

        public int Quantity
        {
            get => quantity;
            set
            {
                quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }

        public string ImageSource
        {
            get => imageSource;
            set
            {
                imageSource = value;
                OnPropertyChanged(nameof(ImageSource));
            }
        }

        // Command Methods
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand RemoveItemCommand { get; }

        public void IncreaseQuantity() => Quantity++;
        public void DecreaseQuantity()
        {
            if (Quantity > 0)
                Quantity--;
        }

        public void RemoveItem()
        {
            // TODO: Hook this to your cart removal logic
            System.Diagnostics.Debug.WriteLine($"Removed {ProductName} from cart.");
        }

        // For BuyerCartItemControl (matches the control's bindings)
        public string Title => ProductName;
        public string Description => $"Quantity: {Quantity}";
        public Action<object> Remove => _ => RemoveItem(); // For Click="{x:Bind ViewModel.Remove}"
        public Action<object> AddToCart => _ => System.Diagnostics.Debug.WriteLine($"Already in cart");

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
