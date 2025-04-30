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

        public Task AddProductToCartAsync(int buyerId, int productId, int quantity)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items";
            var response = this.httpClient.PostAsync(requestUri, null);
            if (response.Result.IsSuccessStatusCode)
            {
                return Task.CompletedTask;
            }
            else
            {
                throw new Exception($"Failed to add product to cart: {response.Result.ReasonPhrase}");
            }
        }

        public Task ClearCartAsync(int buyerId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/shoppingcart/items";
            var response = this.httpClient.DeleteAsync(requestUri);
            if (response.Result.IsSuccessStatusCode)
            {
                return Task.CompletedTask;
            }
            else
            {
                throw new Exception($"Failed to clear cart: {response.Result.ReasonPhrase}");
            }
        }

        public Task<int> GetCartItemCountAsync(int buyerId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/shoppingcart/items";
            var response = this.httpClient.GetAsync(requestUri);
            if (response.Result.IsSuccessStatusCode)
            {
                return response.Result.Content.ReadFromJsonAsync<int>();
            }
            else
            {
                throw new Exception($"Failed to get cart item count: {response.Result.ReasonPhrase}");
            }
        }

        public Task<Dictionary<Product, int>> GetCartItemsAsync(int buyerId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/shoppingcart/items";
            var response = this.httpClient.GetAsync(requestUri);
            if (response.Result.IsSuccessStatusCode)
            {
                return response.Result.Content.ReadFromJsonAsync<Dictionary<Product, int>>();
            }
            else
            {
                throw new Exception($"Failed to get cart items: {response.Result.ReasonPhrase}");
            }
        }

        public Task<int> GetProductQuantityAsync(int buyerId, int productId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/shoppingcart/items/{productId}";
            var response = this.httpClient.GetAsync(requestUri);
            if (response.Result.IsSuccessStatusCode)
            {
                return response.Result.Content.ReadFromJsonAsync<int>();
            }
            else
            {
                throw new Exception($"Failed to get product quantity: {response.Result.ReasonPhrase}");
            }
        }

        public Task<bool> IsProductInCartAsync(int buyerId, int productId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/shoppingcart/items/{productId}";
            var response = this.httpClient.GetAsync(requestUri);
            if (response.Result.IsSuccessStatusCode)
            {
                return response.Result.Content.ReadFromJsonAsync<bool>();
            }
            else
            {
                throw new Exception($"Failed to check if product is in cart: {response.Result.ReasonPhrase}");
            }
        }

        public Task RemoveProductFromCartAsync(int buyerId, int productId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/shoppingcart/items/{productId}";
            var response = this.httpClient.DeleteAsync(requestUri);
            if (response.Result.IsSuccessStatusCode)
            {
                return Task.CompletedTask;
            }
            else
            {
                throw new Exception($"Failed to remove product from cart: {response.Result.ReasonPhrase}");
            }
        }

        public Task UpdateProductQuantityAsync(int buyerId, int productId, int quantity)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/shoppingcart/items/{productId}";
            var response = this.httpClient.PutAsync(requestUri, null);
            if (response.Result.IsSuccessStatusCode)
            {
                return Task.CompletedTask;
            }
            else
            {
                throw new Exception($"Failed to update product quantity: {response.Result.ReasonPhrase}");
            }
        }
    }
}
