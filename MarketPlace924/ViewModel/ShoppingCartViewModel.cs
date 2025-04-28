namespace MarketPlace924.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using CommunityToolkit.Mvvm.Input;
    using MarketPlace924.DBConnection;
    using MarketPlace924.Domain;
    using MarketPlace924.Repository;
    using MarketPlace924.Service;
    using MarketPlace924.ViewModel;

    public class ShoppingCartViewModel : IShoppingCartViewModel
    {
        private readonly IShoppingCartRepository shoppingCartRepository;
        private readonly int buyerId;

        public ObservableCollection<CartItemViewModel> CartItems { get; private set; } = new ObservableCollection<CartItemViewModel>();

        public ICommand RemoveFromCartCommand { get; }

        public ShoppingCartViewModel(IShoppingCartRepository shoppingCartRepository, int buyerId)
        {
            this.shoppingCartRepository = shoppingCartRepository;
            this.buyerId = buyerId;

            RemoveFromCartCommand = new RelayCommand<Product>(async (product) => await DecreaseQuantityAsync(product));
        }

        /// <summary>
        /// Gets the buyer ID associated with this shopping cart.
        /// </summary>
        public int BuyerId => this.buyerId;

        /// <summary>
        /// Gets the total price of all items in the cart.
        /// </summary>
        /// <returns>The total price of all items.</returns>
        public double GetCartTotal()
        {
            double total = 0;
            foreach (var item in this.CartItems)
            {
                total += item.TotalPrice;
            }
            return total;
        }

        /// <summary>
        /// Gets all the products in the cart with their quantities for checkout.
        /// </summary>
        /// <returns>A dictionary containing products and their quantities.</returns>
        public Dictionary<Product, int> GetProductsForCheckout()
        {
            var products = new Dictionary<Product, int>();
            foreach (var item in this.CartItems)
            {
                products.Add(item.Product, item.Quantity);
            }
            return products;
        }

        public async Task LoadCartItemsAsync()
        {
            var cartItemsFromDb = await this.shoppingCartRepository.GetCartItemsAsync(this.buyerId);

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

            var shoppingCartService = new ShoppingCartService(new ShoppingCartRepository(new DatabaseConnection()));
            await shoppingCartService.AddProductToCartAsync(this.buyerId, product.ProductId, quantity);
            await this.LoadCartItemsAsync();
        }

        public async Task RemoveFromCartAsync(Product product)
        {
            await this.shoppingCartRepository.RemoveProductFromCartAsync(this.buyerId, product.ProductId);
            await this.LoadCartItemsAsync();
        }

        public async Task UpdateQuantityAsync(Product product, int quantity)
        {
            await this.shoppingCartRepository.UpdateProductQuantityAsync(this.buyerId, product.ProductId, quantity);
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
                    await this.shoppingCartRepository.UpdateProductQuantityAsync(this.buyerId, product.ProductId, cartItem.Quantity);
                }
                else
                {
                    // if quantity is 1, remove the item from the cart
                    await this.shoppingCartRepository.RemoveProductFromCartAsync(this.buyerId, product.ProductId);
                }

                // Reload the cart items to reflect changes
                await this.LoadCartItemsAsync();
            }
        }
    }
}
