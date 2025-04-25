using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Input;
using Marketplace924.Domain;
using Marketplace924.Repository;
using Marketplace924.Shared;
using MarketPlace924.Domain;

namespace MarketPlace924.ViewModel
{
    public class CartItemViewModel
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public double TotalPrice => Product.Price * Quantity;

        public ICommand RemoveFromCartCommand { get; }

        public CartItemViewModel(Product product, int quantity, ICommand removeFromCartCommand)
        {
            Product = product;
            Quantity = quantity;
            RemoveFromCartCommand = removeFromCartCommand;
        }
    }
}