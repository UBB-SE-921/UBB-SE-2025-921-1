using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using MarketPlace924.Repository;
using System;
using MarketPlace924.Service;
using MarketPlace924.ViewModel;
using System.Windows.Input; 
using CommunityToolkit.Mvvm.Input; 
using System.Linq;

namespace MarketPlace924.ViewModel
{
    public class ShoppingCartViewModel : IShoppingCartViewModel
    {
        private readonly IShoppingCartService shoppingCartService;
        private readonly int buyerId;

        public ObservableCollection<CartItemViewModel> CartItems { get; private set; } = new ObservableCollection<CartItemViewModel>();

        public ICommand RemoveFromCartCommand { get; }

        public ShoppingCartViewModel(IShoppingCartService shoppingCartService, int buyerId)
        {
            this.shoppingCartService = shoppingCartService;
            this.buyerId = buyerId;

            RemoveFromCartCommand = new RelayCommand<Product>(async (product) => await DecreaseQuantityAsync(product));
        }

        public async Task LoadCartItemsAsync()
        {
            var cartItemsFromDb = await this.shoppingCartService.GetCartItemsAsync(this.buyerId);

            this.CartItems.Clear();
            foreach (var item in cartItemsFromDb)
            {
                this.CartItems.Add(new CartItemViewModel(
                    item.Key,
                    item.Value,
                    RemoveFromCartCommand
                ));
            }
        }

        public async Task AddToCartAsync(Product product, int quantity)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product), "Product cannot be null.");
            }

            await this.shoppingCartService.AddProductToCartAsync(this.buyerId, product.ProductId, quantity);
            await this.LoadCartItemsAsync();
        }

        public async Task RemoveFromCartAsync(Product product)
        {
            await this.shoppingCartService.RemoveProductFromCartAsync(this.buyerId, product.ProductId);
            await this.LoadCartItemsAsync();
        }

        public async Task UpdateQuantityAsync(Product product, int quantity)
        {
            await this.shoppingCartService.UpdateProductQuantityAsync(this.buyerId, product.ProductId, quantity);
            await this.LoadCartItemsAsync();
        }
        private async Task DecreaseQuantityAsync(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product), "Product cannot be null.");
            }

            // Find the cart item in the collection
            var cartItem = this.CartItems.FirstOrDefault(item => item.Product.ProductId == product.ProductId);
            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    // decreasing the quantity
                    cartItem.Quantity--;

                    // updating the quantity in the database
                    await this.shoppingCartService.UpdateProductQuantityAsync(this.buyerId, product.ProductId, cartItem.Quantity);
                }
                else
                {
                    // if quantity is 1, remove the item from the cart
                    await this.shoppingCartService.RemoveProductFromCartAsync(this.buyerId, product.ProductId);
                }

                // Reload the cart items to reflect changes
                await this.LoadCartItemsAsync();
            }
        }
    }
}
