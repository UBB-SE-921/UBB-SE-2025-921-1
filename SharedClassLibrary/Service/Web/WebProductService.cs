using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;

namespace SharedClassLibrary.Service.Web
{
    /// <summary>
    /// Service for managing products in the web marketplace
    /// </summary>
    public class WebProductService : IWebProductService
    {
        // This is a placeholder implementation. In a real app, this would use a database
        private static readonly List<Product> Products = new()
        {
            new Product
            {
                ProductId = 1,
                Name = "Digital Camera",
                Price = 299.99,
                SellerId = 101,
                ProductType = "borrowed",
                StartDate = DateTime.Now.AddDays(-5),
                EndDate = DateTime.Now.AddDays(10)
            },
            new Product
            {
                ProductId = 2,
                Name = "Gaming Laptop",
                Price = 1299.99,
                SellerId = 102,
                ProductType = "borrowed",
                StartDate = DateTime.MinValue,
                EndDate = DateTime.MinValue
            },
            new Product
            {
                ProductId = 3,
                Name = "Professional Camera",
                Price = 899.99,
                SellerId = 103,
                ProductType = "borrowed",
                StartDate = DateTime.Now.AddDays(5),
                EndDate = DateTime.MinValue
            }
        };

        private static readonly Dictionary<int, string> Sellers = new()
        {
            { 101, "John Doe" },
            { 102, "Jane Smith" },
            { 103, "Alice Johnson" }
        };

        /// <summary>
        /// Gets a product by ID
        /// </summary>
        /// <param name="productId">The product ID</param>
        /// <returns>The product, or null if not found</returns>
        public Task<Product> GetProductByIdAsync(int productId)
        {
            var product = Products.FirstOrDefault(p => p.ProductId == productId);
            return Task.FromResult(product);
        }

        /// <summary>
        /// Gets a seller's name by ID
        /// </summary>
        /// <param name="sellerId">The seller ID</param>
        /// <returns>The seller name</returns>
        public Task<string> GetSellerNameAsync(int sellerId)
        {
            if (Sellers.TryGetValue(sellerId, out var sellerName))
            {
                return Task.FromResult(sellerName);
            }

            return Task.FromResult($"Seller {sellerId}");
        }
    }
} 