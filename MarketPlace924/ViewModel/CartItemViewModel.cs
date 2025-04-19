using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Windows.Input;
using MarketPlace924.Helper;

namespace MarketPlace924.ViewModel
{
    /// <summary>
    /// Represents a cart item in the marketplace.
    /// </summary>
    public class CartItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string productName;
        private decimal price;
        private int quantity;

        public CartItemViewModel()
        {
            IncreaseQuantityCommand = new RelayCommand(_ => IncreaseQuantity());
            DecreaseQuantityCommand = new RelayCommand(_ => DecreaseQuantity());
            RemoveItemCommand = new RelayCommand(_ => RemoveItem());
        }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the price of the product.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the quantity of the product.
        /// </summary>
        public int Quantity
        {
            get => quantity;
            set
            {
                if (quantity != value)
                {
                   quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                }
            }
        }

        public ICommand IncreaseQuantityCommand { get; }

        public ICommand DecreaseQuantityCommand { get; }

        public ICommand RemoveItemCommand { get; }

        /// <summary>
        /// Command to increment the quantity of the product.
        /// </summary>
        public void IncreaseQuantity()
        {
            Quantity++;
        }

        /// <summary>
        /// Command to decrement the quantity of the product.
        ///</summary>
        public void DecreaseQuantity()
        {
            if (Quantity > 0)
            {
                Quantity--;
            }
        }

        /// <summary>
        /// Command to remove the item from the cart.
        /// </summary>
        public void RemoveItem()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
