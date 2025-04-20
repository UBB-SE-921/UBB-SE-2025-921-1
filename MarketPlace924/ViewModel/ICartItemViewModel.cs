using System;
using System.ComponentModel;
using System.Windows.Input;

namespace MarketPlace924.ViewModel
{
    public interface ICartItemViewModel : INotifyPropertyChanged
    {
        string ProductName { get; set; }
        string Title { get; }                   // Bound in XAML
        string Description { get; }             // Bound in XAML
        decimal Price { get; set; }
        int Quantity { get; set; }
        string ImageSource { get; set; }

        ICommand IncreaseQuantityCommand { get; }
        ICommand DecreaseQuantityCommand { get; }
        ICommand RemoveItemCommand { get; }

        Action<object> Remove { get; }
        Action<object> AddToCart { get; }
    }
}
