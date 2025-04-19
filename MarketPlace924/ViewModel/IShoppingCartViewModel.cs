using MarketPlace924.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

public interface IShoppingCartViewModel : INotifyPropertyChanged
{
    ObservableCollection<CartItemViewModel> CartItems { get; set; }

    ICommand CheckoutCommand { get; set; }

    event Action<string> CheckoutCompleted;

    event PropertyChangedEventHandler PropertyChanged;
}