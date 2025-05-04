using System;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Shared;
using SharedClassLibrary.IRepository;
using SharedClassLibrary.ProxyRepository;
using SharedClassLibrary.IRepository;
using SharedClassLibrary.Helper;
using System.Collections.Generic;
using System.Linq;

namespace SharedClassLibrary.Service
{
    /// <summary>
    /// Service for managing dummy product operations.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository productRepository;

        // This is a placeholder implementation for GetProductByIdAsync and GetSellerNameAsync
        // In a real implementation, this would be stored in the database
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
        /// Initializes a new instance of the <see cref="ProductService"/> class with a specified database provider.
        /// </summary>
        public ProductService()
        {
            this.productRepository = new ProductProxyRepository(AppConfig.GetBaseApiUrl());
        }

        /// <inheritdoc/>
        public async Task UpdateProductAsync(int id, string name, double price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            // Validate inputs
            if (id <= 0)
            {
                throw new ArgumentException("Product ID must be positive", nameof(id));
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Product name cannot be empty", nameof(name));
            }
            if (price < 0)
            {
                throw new ArgumentException("Price cannot be negative", nameof(price));
            }

            if (sellerId < 0)
            {
                throw new ArgumentException("Seller ID cannot be negative", nameof(sellerId));
            }
            if (string.IsNullOrWhiteSpace(productType))
            {
                throw new ArgumentException("Product type cannot be empty", nameof(productType));
            }

            // Only validate start and end dates for borrowed products
            if (productType == "borrowed")
            {
                if (startDate > endDate)
                {
                    throw new ArgumentException("Start date cannot be after end date", nameof(startDate));
                }
            }
            await productRepository.UpdateProductAsync(id, name, price, sellerId, productType, startDate, endDate);
        }

        /// <inheritdoc/>
        public Task<Product> GetProductByIdAsync(int productId)
        {
            var product = Products.FirstOrDefault(p => p.ProductId == productId);
            return Task.FromResult(product);
        }

        /// <inheritdoc/>
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
