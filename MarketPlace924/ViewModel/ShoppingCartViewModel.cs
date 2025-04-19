using MarketPlace924.Helper;
using MarketPlace924.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System;

public class ShoppingCartViewModel : IShoppingCartViewModel
{
    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<CartItemViewModel> CartItems { get; set; }

    // Declare the event that will notify the View
    public event Action<string> CheckoutCompleted;

    // Command for checkout process
    public ICommand CheckoutCommand { get; set; }

    public ShoppingCartViewModel()
    {
        // Initialize the cart with some sample items (replace with actual data)
        CartItems = new ObservableCollection<CartItemViewModel>
        {
            new CartItemViewModel { ProductName = "Laptop", Price = 1000, Quantity = 1 },
            new CartItemViewModel { ProductName = "Smartphone", Price = 500, Quantity = 2 }
        };

        // Initialize the Checkout command using RelayCommand
        CheckoutCommand = new RelayCommand(OnCheckout, CanCheckout);
    }

    // Logic to check whether checkout can happen (e.g., if cart is not empty)
    private bool CanCheckout()
    {
        return CartItems.Count > 0;
    }

    // Handle the checkout process
    private void OnCheckout()
    {
        decimal totalAmount = 0;
        foreach (var item in CartItems)
        {
            totalAmount += item.Price * item.Quantity;
        }

        if (CartItems.Count == 0)
        {
            // Trigger the event to notify the View that checkout failed (empty cart)
            CheckoutCompleted?.Invoke("Your cart is empty. Please add some items before checking out.");
            return;
        }

        // Simulate checkout process
        bool checkoutSuccessful = SimulateCheckout(totalAmount);

        if (checkoutSuccessful)
        {
            // Trigger the event to notify the View that checkout was successful
            CheckoutCompleted?.Invoke($"Checkout successful! Total: ${totalAmount}");
            CartItems.Clear(); // Clear the cart after checkout
        }
        else
        {
            // Trigger the event to notify the View that checkout failed
            CheckoutCompleted?.Invoke("Checkout failed. Please try again.");
        }
    }

    // Placeholder method to simulate a checkout process
    private bool SimulateCheckout(decimal totalAmount)
    {
        // Simulate a successful checkout
        return totalAmount > 0;
    }

    // Helper method to notify that a property has changed (for INotifyPropertyChanged implementation)
    private void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
