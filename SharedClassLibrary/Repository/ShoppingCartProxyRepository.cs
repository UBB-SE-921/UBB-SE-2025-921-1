using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.IRepository;
using System.Net.Http;
using System.Net.Http.Json;

namespace MarketPlace924.Repository
{
    internal class ShoppingCartProxyRepository : IShoppingCartRepository
    {
        private const string ApiBaseRoute = "api/shoppingcart";
        private readonly HttpClient httpClient;

        public ShoppingCartProxyRepository(string baseApiUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new System.Uri(baseApiUrl);
        }

        public async Task AddProductToCartAsync(int buyerId, int productId, int quantity)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items?productId={productId}&quantity={quantity}";
            var response = await this.httpClient.PostAsync(requestUri, null);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to add product to cart: {response.ReasonPhrase}");
            }
        }

        public async Task ClearCartAsync(int buyerId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items";
            var response = await this.httpClient.DeleteAsync(requestUri);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to clear cart: {response.ReasonPhrase}");
            }
        }

        public async Task<int> GetCartItemCountAsync(int buyerId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items/count";
            var response = await this.httpClient.GetAsync(requestUri);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<int>();
            }
            else
            {
                throw new Exception($"Failed to get cart item count: {response.ReasonPhrase}");
            }
        }

        public async Task<Dictionary<Product, int>> GetCartItemsAsync(int buyerId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items";
            var response = await this.httpClient.GetAsync(requestUri);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Dictionary<Product, int>>();
                if (result == null)
                {
                    result = new Dictionary<Product, int>();
                }
                return result;
            }
            else
            {
                throw new Exception($"Failed to get cart items: {response.ReasonPhrase}");
            }
        }

        public async Task<int> GetProductQuantityAsync(int buyerId, int productId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items/{productId}/quantity";
            var response = await this.httpClient.GetAsync(requestUri);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<int>();
            }
            else
            {
                throw new Exception($"Failed to get product quantity: {response.ReasonPhrase}");
            }
        }

        public async Task<bool> IsProductInCartAsync(int buyerId, int productId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items/{productId}/exists";
            var response = await this.httpClient.GetAsync(requestUri);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<bool>();
            }
            else
            {
                throw new Exception($"Failed to check if product is in cart: {response.ReasonPhrase}");
            }
        }

        public async Task RemoveProductFromCartAsync(int buyerId, int productId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items/{productId}";
            var response = await this.httpClient.DeleteAsync(requestUri);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to remove product from cart: {response.ReasonPhrase}");
            }
        }

        public async Task UpdateProductQuantityAsync(int buyerId, int productId, int quantity)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items/{productId}?quantity={quantity}";
            var response = await this.httpClient.PutAsync(requestUri, null);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to update product quantity: {response.ReasonPhrase}");
            }
        }
    }
}
