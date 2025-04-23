using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MarketPlace924.Domain;
using MarketPlace924.Repository;

namespace MarketPlace924.ViewModel
{
    public class ShoppingCartViewModel : IShoppingCartViewModel
    {
        private readonly IShoppingCartRepository shoppingCartRepository;
        private readonly int buyerId;

        public ObservableCollection<CartItemViewModel> CartItems { get; private set; } = new ObservableCollection<CartItemViewModel>();

        public ShoppingCartViewModel(IShoppingCartRepository shoppingCartRepository, int buyerId)
        {
            this.shoppingCartRepository = shoppingCartRepository;
            this.buyerId = buyerId;
        }

        public async Task LoadCartItemsAsync()
        {
            var cartItems = await this.shoppingCartRepository.GetCartItemsAsync(this.buyerId);
            this.CartItems.Clear();

            foreach (var item in cartItems)
            {
                this.CartItems.Add(new CartItemViewModel
                {
                    Product = item.Key,
                    Quantity = item.Value
                });
            }
        }

        public async Task AddToCartAsync(Product product, int quantity)
        {
            await this.shoppingCartRepository.AddProductToCartAsync(this.buyerId, product.ProductId, quantity);
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
    }

    public class CartItemViewModel
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
    }
}
