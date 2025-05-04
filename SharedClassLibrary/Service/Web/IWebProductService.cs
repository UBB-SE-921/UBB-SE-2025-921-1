using System.Threading.Tasks;
using SharedClassLibrary.Domain;

namespace SharedClassLibrary.Service.Web
{
    /// <summary>
    /// Service for managing web products
    /// </summary>
    public interface IWebProductService
    {
        /// <summary>
        /// Gets a product by ID
        /// </summary>
        /// <param name="productId">The product ID</param>
        /// <returns>The product, or null if not found</returns>
        Task<Product> GetProductByIdAsync(int productId);

        /// <summary>
        /// Gets a seller's name by ID
        /// </summary>
        /// <param name="sellerId">The seller ID</param>
        /// <returns>The seller name</returns>
        Task<string> GetSellerNameAsync(int sellerId);
    }
} 